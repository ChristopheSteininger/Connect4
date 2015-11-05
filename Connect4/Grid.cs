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

        // This three dimensional table is accessed by height, width then player.
        private ulong[] zobristTable;

        // The hash of the current position.
        private ulong hash = 0;
        public ulong Hash { get { return hash; } }

        // The hash of the mirror image of the current position.
        private ulong flippedHash = 0;
        public ulong FlippedHash { get { return flippedHash; } }

        // Each ulong represents a player's pieces on the board. Each height + 1
        // bits represent a column from bottom to top. If the bit is 0, then the
        // player does not have a piece on that position.
        private ulong[] playerPositions;
        public TileState this[int row, int column]
        {
            get { return GetTileState(row, column); }
        }

        public const ulong bottomRow = 0x40810204081;

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

            InitialiseZobristTable();
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

        private void InitialiseZobristTable()
        {
            Random random = new Random(seed);
            zobristTable = new ulong[height * width * 2];

            for (int y = 0; y < zobristTable.Length; y++)
            {
                byte[] randomBytes = new byte[sizeof(ulong)];
                random.NextBytes(randomBytes);

                zobristTable[y] = BitConverter.ToUInt64(randomBytes, 0);
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
        /// Also sets the bottom most row and the tops of full columnns to 1.
        /// </summary>
        public ulong GetValidMovesMask()
        {
            return ((playerPositions[0] | playerPositions[1]) << 1) | bottomRow;
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

            // Update the hash values.
            hash ^= zobristTable[(player * width * height) + (column * height) + row];
            flippedHash ^= zobristTable[
                (player * width * height) + ((width - column - 1) * height) + row];

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

            // Restore the hash values.
            hash ^= zobristTable[(player * width * height) + (column * height) + row];
            flippedHash ^= zobristTable[
                (player * width * height) + ((width - column - 1) * height) + row];
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
