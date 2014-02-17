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

        private ulong[][][] zobristTable;
        private ulong hash = 0;

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

            SetStreakMasks();
            InitialiseZobristTable();
        }

        private void InitialiseZobristTable()
        {
            Random random = new Random();
            zobristTable = new ulong[height][][];

            for (int y = 0; y < height; y++)
            {
                zobristTable[y] = new ulong[width][];
                for (int x = 0; x < width; x++)
                {
                    zobristTable[y][x] = new ulong[2];
                    for (int player = 0; player < 2; player++)
                    {
                        byte[] randomBytes = new byte[sizeof(ulong)];
                        random.NextBytes(randomBytes);

                        zobristTable[y][x][player] =
                            BitConverter.ToUInt64(randomBytes, 0);
                    }
                }
            }
        }

        private TileState GetTileState(int row, int column)
        {
            ulong mask = (ulong)1 << (column + row * width);
            if ((playerPositions[0] & mask) == mask)
            {
                return TileState.Player1;
            }

            if ((playerPositions[1] & mask) == mask)
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

        private void ClearTile(int row, int column)
        {
            Debug.Assert(GetTileState(row, column) != TileState.Empty);

            ulong mask = ~((ulong)1 << (column + row * width));
            playerPositions[0] &= mask;
            playerPositions[1] &= mask;
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

        public void Move(int column, int player)
        {
            Debug.Assert(0 <= column && column < width);
            Debug.Assert(nextFreeTile[column] < height);

            // Update the hash value.
            hash ^= zobristTable[nextFreeTile[column]][column][player];

            // Update the board.
            SetTileState((TileState)player, nextFreeTile[column], column);
            nextFreeTile[column]++;

            lastMove = column;
        }

        public void UndoMove(int column, int player)
        {
            Debug.Assert(0 <= column && column < width);
            Debug.Assert(0 < nextFreeTile[column] && nextFreeTile[column] <= height);
            Debug.Assert(GetTileState(nextFreeTile[column] - 1, column) == (TileState)player);

            nextFreeTile[column]--;

            // Restore the hash value.
            hash ^= zobristTable[nextFreeTile[column]][column][player];

            // Restore the board.
            ClearTile(nextFreeTile[column], column);

            lastMove = -1;
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

        public ulong GetTTableHash()
        {
            return hash;
        }

        public override int GetHashCode()
        {
            return (int)(hash % int.MaxValue);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Grid other = (Grid)obj;
            return width == other.width && height == other.height
                && playerPositions[0] == other.playerPositions[0]
                && playerPositions[1] == other.playerPositions[1];
        }
    }
}
