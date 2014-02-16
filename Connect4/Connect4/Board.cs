using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Linq;

namespace Connect4
{
    class Board
        : DrawableGameComponent
    {
        private int currentPlayer = 0;
        private Player[] players = new Player[2];

        private Grid grid;

        private MouseState oldMouseState;

        private int highlighedColumn;
        private int boardStartX;
        private int boardStartY;
        private SpriteBatch spriteBatch;
        private Texture2D disc;

        public Board(int gridWidth, int gridHeight, Player player1, Player player2,
            Game game)
            : base(game)
        {
            this.players[0] = player1;
            this.players[1] = player2;

            this.grid = new Grid(gridWidth, gridHeight);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            disc = Game.Content.Load<Texture2D>("disc");

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            PlayTurn();

            oldMouseState = Mouse.GetState();

            base.Update(gameTime);
        }

        private void PlayTurn()
        {
            Debug.Assert(currentPlayer == 0 || currentPlayer == 1);

            if (grid.IsGameOver() == -1)
            {
                highlighedColumn = players[currentPlayer].HighlightColumn(
                    boardStartX, boardStartY, boardStartY + grid.Height * disc.Height,
                    disc.Width);
                int move = players[currentPlayer].GetMove(grid);

                if (grid.IsValidMove(move))
                {
                    grid = grid.Move(move, currentPlayer);

                    currentPlayer = 1 - currentPlayer;
                    highlighedColumn = -1;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            boardStartX = (GraphicsDevice.Viewport.Width - grid.Width * disc.Width) / 2;
            boardStartY = (GraphicsDevice.Viewport.Height - grid.Height * disc.Height) / 2;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    Color color = Color.White;
                    Color[] playerColors = { Color.Yellow, Color.Red };
                    Color[] fadedPlayerColors = { new Color(150, 150, 0),
                                                    new Color(150, 0, 0) };

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

                    Vector2 position = new Vector2(boardStartX + x * disc.Width,
                        boardStartY + (grid.Height - y - 1) * disc.Height);

                    spriteBatch.Draw(disc, position, color);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
