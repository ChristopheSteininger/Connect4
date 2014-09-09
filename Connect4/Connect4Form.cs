using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Connect4
{
    public partial class Connect4Form : Form, IBoardDrawer
    {
        private Board board;

        private int boardStartX;
        private int boardStartY;

        private const int width = 7;
        private const int height = 6;

        private Image disc;

        public Connect4Form()
        {
            InitializeComponent();

            LoadImages();

            board = new Board(width, height, this);
            board.OnStart();
        }

        private void LoadImages()
        {
            const string discImageLocation = "../../disc.png";

            Debug.Assert(File.Exists(discImageLocation));
            disc = Image.FromFile(discImageLocation);
        }

        private void plBoard_MouseClick(object sender, MouseEventArgs e)
        {
            board.OnClick();
        }

        private void plBoard_MouseMove(object sender, MouseEventArgs e)
        {
            int mouseY = plBoard.Height - e.Y;
            int mouseX = e.X;

            if (boardStartY <= mouseY && mouseY <= boardStartY + height * disc.Height
                && mouseX >= boardStartX && !board.IsGameOver)
            {
                board.HighlightedColumn = (e.X - boardStartX) / disc.Width;
            }

            else
            {
                board.HighlightedColumn = -1;
            }

            DrawBoard();
        }

        private void plBoard_Paint(object sender, PaintEventArgs e)
        {
            DrawBoard();
        }

        public void DrawBoard()
        {
            Graphics graphics = plBoard.CreateGraphics();

            // TODO: Lock the grid?
            boardStartX = (plBoard.Width - width * disc.Width) / 2;
            boardStartY = (plBoard.Height - height * disc.Height) / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color[] playerColors = { Color.Yellow, Color.Red, Color.White };
                    Color[] fadedPlayerColors = { Color.FromArgb(150, 150, 0),
                                                    Color.FromArgb(150, 0, 0) };

                    Color color = playerColors[(int)board[y, x]];

                    if (board[y, x] == TileState.Empty && x == board.HighlightedColumn)
                    {
                        if (board.IsValidMove(x, y))
                        {
                            color = fadedPlayerColors[board.CurrentPlayer];
                        }

                        else
                        {
                            color = Color.LightGray;
                        }
                    }

                    Point position = new Point(boardStartX + x * disc.Width,
                        boardStartY + (height - y - 1) * disc.Height);

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
