using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Connect4
{
    enum TileState { Player1 = 0, Player2 = 1, Empty = 2 }

    [Serializable()]
    partial class Grid
    {
        private readonly int width;
        public int Width { get { return width; } }

        private readonly int height;
        public int Height { get { return height; } }

        private int[] nextFreeTile;

        private int seed;

        // The hash of the current position.
        public ulong Hash
        {
            get
            {
                return ((playerPositions[0] | playerPositions[1]) + bottomRow) | playerPositions[0];
            }
        }

        // The hash of the mirror image of the current position.
        public ulong FlippedHash
        {
            get
            {
                ulong hash = Hash;
                ulong left1  = ((0x7FUL << (0 * (height + 1))) & hash) << (6 * (height + 1));
                ulong left2  = ((0x7FUL << (1 * (height + 1))) & hash) << (4 * (height + 1));
                ulong left3  = ((0x7FUL << (2 * (height + 1))) & hash) << (2 * (height + 1));
                ulong middle = ((0x7FUL << (3 * (height + 1))) & hash) << (0 * (height + 1));
                ulong right3 = ((0x7FUL << (4 * (height + 1))) & hash) >> (2 * (height + 1));
                ulong right2 = ((0x7FUL << (5 * (height + 1))) & hash) >> (4 * (height + 1));
                ulong right1 = ((0x7FUL << (6 * (height + 1))) & hash) >> (6 * (height + 1));

                return left1 | left2 | left3 | middle | right3 | right2 | right1;
            }
        }

        // Each ulong represents a player's pieces on the board. Each height + 1
        // bits represent a column from bottom to top. If the bit is 0, then the
        // player does not have a piece on that position.
        private ulong[] playerPositions;
        public TileState this[int row, int column]
        {
            get { return GetTileState(row, column); }
        }

        public const ulong bottomRow = 0x40810204081;
        public const ulong allRows = bottomRow
            | (bottomRow << 1)
            | (bottomRow << 2)
            | (bottomRow << 3)
            | (bottomRow << 4)
            | (bottomRow << 5);
        public const ulong oddRows = bottomRow | (bottomRow << 2) | (bottomRow << 4);
        public const ulong evenRows = oddRows << 1;

        private string AA1 { get { return PrintRow(5); } }
        private string AA2 { get { return PrintRow(4); } }
        private string AA3 { get { return PrintRow(3); } }
        private string AA4 { get { return PrintRow(2); } }
        private string AA5 { get { return PrintRow(1); } }
        private string AA6 { get { return PrintRow(0); } }

        public Grid(int width, int height, int seed)
        {
            Debug.Assert(width > 4);
            Debug.Assert(height > 4);
            Debug.Assert(width * height <= 64);

            this.width = width;
            this.height = height;

            this.playerPositions = new ulong[2] { 0, 0 };

            this.nextFreeTile = new int[width];
            for (int y = 0; y < height; y++)
            {
                nextFreeTile[y] = 0;
            }

            this.seed = seed;
        }

        // Only to be used by the board at the start of a move.
        public Grid Clone()
        {
            Debug.Assert(typeof(Grid).IsSerializable);

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, this);
                stream.Seek(0, SeekOrigin.Begin);

                return (Grid)formatter.Deserialize(stream);
            }
        }

        private TileState GetTileState(int row, int column)
        {
            ulong mask = (ulong)1 << (column * (height + 1) + row);
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

        public bool IsValidMove(int column, int row)
        {
            return IsValidMove(column) && 0 <= row && row < height
                && nextFreeTile[column] == row;
        }

        public ulong GetInvalidMovesMask()
        {
            return ((playerPositions[0] | playerPositions[1]) >> (height - 1)) & bottomRow;
        }

        /// <summary>
        /// Returns a board with a 1 in each location corresponding to a valid move.
        /// </summary>
        public ulong GetValidMovesMask()
        {
            return (((playerPositions[0] | playerPositions[1]) << 1) + bottomRow) & allRows;
        }

        public bool IsValidMove(int column)
        {
            return 0 <= column && column < width && nextFreeTile[column] < height;
        }

        public void Move(int column, int player)
        {
            Debug.Assert(0 <= column && column < width);

            int row = nextFreeTile[column]++;
            Debug.Assert(row < height);
            Debug.Assert(GetTileState(row, column) == TileState.Empty);

            // Update the board.
            playerPositions[player] |= 1UL << (column * (height + 1) + row);
        }

        public void UndoMove(int column, int player)
        {
            Debug.Assert(0 <= column && column < width);

            int row = --nextFreeTile[column];
            Debug.Assert(0 <= row && row < height);
            Debug.Assert(GetTileState(row, column) != TileState.Empty);

            // Restore the board.
            playerPositions[player] &= ~(1UL << (column * (height + 1) + row));
        }

        public override string ToString()
        {
            string result = "";

            for (int row = height - 1; row >= 0; row--)
            {
                result += "|" + PrintRow(row) + "|" + Environment.NewLine;
            }

            string horizontalBorder = "+" + new string('-', width) + "+";
            return horizontalBorder + Environment.NewLine + result + horizontalBorder;
        }

        private string PrintRow(int row)
        {
            string result = "";

            for (int column = 0; column < width; column++)
            {
                switch (GetTileState(row, column))
                {
                    case TileState.Empty:
                        result += "\u00B7";
                        break;

                    case TileState.Player1:
                        result += "\u25CB";
                        break;

                    case TileState.Player2:
                        result += "\u25CF";
                        break;
                }
            }

            return result;
        }

        public override int GetHashCode()
        {
            return (int)(Hash % int.MaxValue);
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
