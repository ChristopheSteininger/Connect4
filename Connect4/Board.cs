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

        private int highlighedColumn = 3;
        private int boardStartX;
        private int boardStartY;

        private Panel panel;
        private Image disc;

        public Board(int gridWidth, int gridHeight, Panel panel)
        {
            //int seed = 1092552428;
            //int seed = 2053617222;
            int seed = new Random().Next();

            const int AIPlayer = 0;
            const int humanPlayer = 1;

            this.players[AIPlayer] = new AIPlayer(AIPlayer, seed);
            this.players[humanPlayer] = new HumanPlayer(humanPlayer);

            this.grid = new Grid(gridWidth, gridHeight, seed);
            grid.Move(3, 0);
            grid.Move(0, 1);
            grid.Move(6, 0);

            this.panel = panel;

            const string discImageLocation = "../../disc.png";
            Debug.Assert(File.Exists(discImageLocation));
            this.disc = Image.FromFile(discImageLocation);

            panel.Paint += new PaintEventHandler(panel_Paint);
        }

        public void Update()
        {
            if (winner == -1)
            {
                PlayTurn();

                // If the game ended on this turn, let both players know.
                if (winner != -1)
                {
                    players[0].GameOver(winner == 0);
                    players[1].GameOver(winner == 1);
                }
            }
        }

        private void PlayTurn()
        {
            Debug.Assert(currentPlayer == 0 || currentPlayer == 1);

            winner = grid.IsGameOver();

            if (winner == -1)
            {
                highlighedColumn = players[currentPlayer].HighlightColumn(
                    boardStartX, boardStartY, boardStartY + grid.Height * disc.Height,
                    disc.Width);
                int move = players[currentPlayer].GetMove(grid);

                if (grid.IsValidMove(move))
                {
                    grid.Move(move, currentPlayer);

                    currentPlayer = 1 - currentPlayer;
                    highlighedColumn = -1;
                }
            }
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            Draw();
        }

        private void Draw()
        {
            Graphics graphics = panel.CreateGraphics();
            graphics.Clear(panel.BackColor);

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
