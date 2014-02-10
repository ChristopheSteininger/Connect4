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

        private SpriteBatch spriteBatch;
        private Texture2D disc;

        public Board(int gridSize, Player player1, Player player2, Game game)
            : base(game)
        {
            Debug.Assert(gridSize > 0);

            this.players[0] = player1;
            this.players[1] = player2;

            this.grid = new Grid(gridSize);
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
                int move = players[currentPlayer].GetMove(grid);
                if (grid.IsValidMove(move))
                {
                    grid = grid.Move(move, currentPlayer);
                    currentPlayer = 1 - currentPlayer;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            for (int y = 0; y < grid.Size; y++)
            {
                for (int x = 0; x < grid.Size; x++)
                {
                    Color color = Color.White;
                    if (grid[y, x] != TileState.Empty)
                    {
                        color = grid[y, x] == TileState.Player1 ? Color.Yellow : Color.Red;
                    }

                    spriteBatch.Draw(disc, new Vector2(x * disc.Width,
                        GraphicsDevice.Viewport.Height - (y + 1) * disc.Height), color);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
