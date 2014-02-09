using System;
using System.Diagnostics;

namespace Connect4
{
    enum TileState { Player1, Player2, Empty }

    partial class Grid
    {
        private readonly int size;
        public int Size
        {
            get { return size; }
        }

        private int[] nextFreeTile;

        private TileState[,] grid;
        public TileState this[int row, int column]
        {
            get { return grid[row, column]; }
        }

        public Grid(int size)
        {
            Debug.Assert(size > 4);

            this.size = size;
            this.grid = new TileState[size, size];
            this.nextFreeTile = new int[size];
            for (int y = 0; y < size; y++)
            {
                nextFreeTile[y] = 0;
                for (int x = 0; x < size; x++)
                {
                    this.grid[y, x] = TileState.Empty;
                }
            }
        }

        private Grid(Grid grid)
        {
            this.size = grid.size;
            this.grid = new TileState[size, size];
            this.nextFreeTile = new int[size];
            for (int y = 0; y < size; y++)
            {
                this.nextFreeTile[y] = grid.nextFreeTile[y];
                for (int x = 0; x < size; x++)
                {
                    this.grid[y, x] = grid.grid[y, x];
                }
            }
        }

        public bool IsValidMove(int column)
        {
            return nextFreeTile[column] < size;
        }

        public int[] GetPlayerStreaks(int player)
        {
            Debug.Assert(player == 0 || player == 1);

            TileState playerTile = (TileState)player;
            int[] result = new int[3]; // Streaks of 2, 3 and > 4.
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = 0;
            }

            for (int row = 0; row < size; row++)
            {
                int currentStart = -1;
                for (int column = 0; column < size; column++)
                {
                    // If this could be the start of a new streak.
                    if (grid[row, column] == playerTile && currentStart == -1)
                    {
                        currentStart = column;
                    }

                    // If this is the end of any streak.
                    else if ((grid[row, column] != playerTile || column == size - 1)
                        && currentStart != -1)
                    {
                        // If the streak was longer than one tile.
                        if (column - currentStart > 1)
                        {
                            result[Math.Min(column - currentStart - 2, result.Length - 1)]++;
                        }

                        currentStart = -1;
                    }
                }
            }

            return result;
        }

        public Grid Move(int column, int player)
        {
            Debug.Assert(0 <= column && column < size);
            Debug.Assert(nextFreeTile[column] < size);

            Grid result = new Grid(this);

            TileState state = player == 0 ? TileState.Player1 : TileState.Player2;
            result.grid[nextFreeTile[column], column] = state;

            result.nextFreeTile[column]++;

            return result;
        }

        public override string ToString()
        {
            string result = "";

            for (int row = size - 1; row >= 0; row--)
            {
                for (int column = 0; column < size; column++)
                {
                    switch (grid[row, column])
                    {
                        case TileState.Empty:
                            result += "-";
                            break;

                        case TileState.Player1:
                            result += "1";
                            break;

                        case TileState.Player2:
                            result += "2";
                            break;
                    }
                }

                result += "\n";
            }

            return result;
        }
    }
}
