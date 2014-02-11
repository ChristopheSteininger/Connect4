using System;
using System.Diagnostics;

namespace Connect4
{
    enum TileState { Player1 = 0, Player2 = 1, Empty = 2 }

    partial class Grid
    {
        private readonly int width;
        public int Width
        {
            get { return width; }
        }

        private readonly int height;
        public int Height
        {
            get { return height; }
        }

        private int[] nextFreeTile;

        // Each ulong represents a player's pieces on the board. The first width
        // bits represent the bottom row, if the bit is 0, then the player does
        // not have a piece on that position otherwise the player does have piece
        // at that position. The next width bits represent the second row and so
        // on.
        private ulong[] playerPositions;
        public TileState this[int row, int column]
        {
            get { return GetTileState(row, column); }
        }

        public Grid(int width, int height)
        {
            Debug.Assert(width > 4);
            Debug.Assert(height > 4);
            Debug.Assert(width * height <= 64);

            this.width = width;
            this.height = height;

            this.nextFreeTile = new int[width];
            for (int y = 0; y < height; y++)
            {
                nextFreeTile[y] = 0;
            }

            this.playerPositions = new ulong[2] { 0, 0 };
        }

        private Grid(Grid grid)
        {
            this.width = grid.width;
            this.height = grid.height;

            this.playerPositions = new ulong[2] {
                grid.playerPositions[0], grid.playerPositions[1] };

            this.nextFreeTile = new int[width];
            for (int x = 0; x < width; x++)
            {
                this.nextFreeTile[x] = grid.nextFreeTile[x];
            }
        }

        private TileState GetTileState(int row, int column)
        {
            if (PlayerPieceAtPosition(0, row, column))
            {
                return TileState.Player1;
            }

            else if (PlayerPieceAtPosition(1, row, column))
            {
                return TileState.Player2;
            }

            return TileState.Empty;
        }

        private void SetTileState(TileState state, int row, int column)
        {
            Debug.Assert(GetTileState(row, column) == TileState.Empty);

            ulong mask = (ulong)1 << (column + row * width);
            playerPositions[(int)state] |= mask;
        }

        private bool PlayerPieceAtPosition(int player, int row, int column)
        {
            ulong mask = (ulong)1 << (column + row * width);
            return (playerPositions[player] & mask) == mask;
        }

        public bool IsValidMove(int column, int row)
        {
            return IsValidMove(column) && 0 <= row && row < height
                && nextFreeTile[column] == row;
        }

        public bool IsValidMove(int column)
        {
            return 0 <= column && column < width && nextFreeTile[column] < height;
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

            for (int row = 0; row < height; row++)
            {
                int currentStart = -1;
                for (int column = 0; column < width; column++)
                {
                    // If this could be the start of a new streak.
                    if (GetTileState(row, column) == playerTile && currentStart == -1)
                    {
                        currentStart = column;
                    }

                    // If this is the end of any streak.
                    else if ((GetTileState(row, column) != playerTile || column == width - 1)
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
            Debug.Assert(0 <= column && column < width);
            Debug.Assert(nextFreeTile[column] < height);

            Grid result = new Grid(this);

            result.SetTileState((TileState)player, nextFreeTile[column], column);
            result.nextFreeTile[column]++;

            return result;
        }

        public override string ToString()
        {
            string result = "";

            for (int row = height - 1; row >= 0; row--)
            {
                for (int column = 0; column < width; column++)
                {
                    switch (GetTileState(row, column))
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
