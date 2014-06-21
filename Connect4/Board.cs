using System;

namespace Connect4
{
    class Board
    {
        private int currentPlayer = 0;
        private Player[] players = new Player[2];

        private Grid grid;
        private int winner = -1;

        private IBoardDrawer drawer;

        private int seed;

        private int highlightedColumn = 3;

        public int Seed { get { return seed; } }
        public int CurrentPlayer { get { return currentPlayer; } }
        public bool IsGameOver { get { return winner != -1; } }
        public Grid Grid { get { return grid; } }

        public int HighlightedColumn
        {
            get { return highlightedColumn; }
            set { highlightedColumn = value; }
        }

        public Board(int gridWidth, int gridHeight, IBoardDrawer drawer)
        {
            this.drawer = drawer;

            const int AIPlayer = 0;
            const int humanPlayer = 1 - AIPlayer;

            // Test seeds:
            //seed = 1092552428;
            //seed = 2053617222;
            seed = new Random().Next();

            players[AIPlayer] = new AIPlayer(AIPlayer, this);
            players[humanPlayer] = new HumanPlayer(humanPlayer, this);

            this.grid = new Grid(gridWidth, gridHeight, seed);
        }

        public void OnStart()
        {
            players[currentPlayer].BeginMove();
        }

        public void OnClick()
        {
            players[currentPlayer].OnClick();
        }

        public void MoveWithHighlightedColumn(int player)
        {
            Move(highlightedColumn, player);
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
            highlightedColumn = -1;

            winner = grid.IsGameOver();
            if (winner != -1)
            {
                players[0].GameOver(winner == 0);
                players[1].GameOver(winner == 1);
            }

            drawer.DrawBoard();

            players[currentPlayer].BeginMove();
        }
    }
}
