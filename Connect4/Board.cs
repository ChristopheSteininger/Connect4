using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Connect4
{
    class Board
    {
        private int currentPlayer = 0;
        private Player[] players = new Player[2];

        private Grid grid;
        private int winner = -1;

        private int seed;

        private int highlighedColumn = 3;
        private int boardStartX;
        private int boardStartY;

        private Panel panel;
        private Image disc;

        public int Seed { get { return seed; } }
        public Panel BoardPanel { get { return panel; } }
        public Grid Grid { get { return grid; } }

        public Board(int gridWidth, int gridHeight, Panel panel)
        {
            this.panel = panel;
            panel.Paint += new PaintEventHandler(panel_Paint);
            panel.MouseMove += new MouseEventHandler(panel_MouseMove);

            const string discImageLocation = "../../disc.png";
            Debug.Assert(File.Exists(discImageLocation));
            this.disc = Image.FromFile(discImageLocation);

            //seed = 1092552428;
            //seed = 2053617222;
            seed = new Random().Next();

            const int AIPlayer = 0;
            const int humanPlayer = 1 - AIPlayer;

            players[AIPlayer] = new AIPlayer(AIPlayer, this);
            players[humanPlayer] = new HumanPlayer(humanPlayer, this);

            this.grid = new Grid(gridWidth, gridHeight, seed);

            players[currentPlayer].BeginMove();
        }

        public void MoveWithHighlightedColumn(int player)
        {
            Move(highlighedColumn, player);
        }

        public void Move(int move, int player)
        {
            // Ignore the move if it is the wrong player, not a valid move
            // or if the game is over.
            if (player != currentPlayer || !grid.IsValidMove(move)
                || winner != -1)
            {
                return;
            }

            grid.Move(move, player);
            currentPlayer = 1 - currentPlayer;
            highlighedColumn = -1;

            winner = grid.IsGameOver();
            if (winner != -1)
            {
                players[0].GameOver(winner == 0);
                players[1].GameOver(winner == 1);
            }

            Draw();

            players[currentPlayer].BeginMove();
        }

        void panel_MouseMove(object sender, MouseEventArgs e)
        {
            int mouseY = panel.Height - e.Y;
            int mouseX = e.X;
            if (boardStartY <= mouseY && mouseY <= boardStartY + grid.Height * disc.Height
                && mouseX >= boardStartX && winner == -1)
            {
                highlighedColumn = (e.X - boardStartX) / disc.Width;
            }

            else
            {
                highlighedColumn = -1;
            }

            Draw();
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            Graphics graphics = panel.CreateGraphics();

            boardStartX = (panel.Width - grid.Width * disc.Width) / 2;
            boardStartY = (panel.Height - grid.Height * disc.Height) / 2;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Color color = Color.White;
                    Color[] playerColors = { Color.Yellow, Color.Red };
                    Color[] fadedPlayerColors = { Color.FromArgb(150, 150, 0),
                                                    Color.FromArgb(150, 0, 0) };

                    if (grid[y, x] != TileState.Empty)
                    {
                        color = playerColors[(int)grid[y, x]];
                    }

                    else if (x == highlighedColumn)
                    {
                        if (grid.IsValidMove(x, y))
                        {
                            color = fadedPlayerColors[currentPlayer];
                        }
                        else
                        {
                            color = Color.LightGray;
                        }
                    }

                    Point position = new Point(boardStartX + x * disc.Width,
                        boardStartY + (grid.Height - y - 1) * disc.Height);

                    graphics.DrawImage(disc, new Rectangle(position,
                        new Size(disc.Width, disc.Height)), 0, 0, disc.Width, disc.Height,
                        GraphicsUnit.Pixel, GetTintAttributes(color));
                }
            }
        }

        private ImageAttributes GetTintAttributes(Color tint)
        {
            float r = (float)(tint.R / 255.0);
            float g = (float)(tint.G / 255.0);
            float b = (float)(tint.B / 255.0);

            ColorMatrix colorMatrix = new ColorMatrix(new float[][] {
                new float[] { r, 0, 0, 0, 0 },
                new float[] { 0, g, 0, 0, 0 },
                new float[] { 0, 0, b, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);

            return attributes;
        }
    }
}
