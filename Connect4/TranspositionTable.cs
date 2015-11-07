using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    class TranspositionTable
    {
        private ulong[] table = new ulong[TableSize];

        // The number of most significant bits of a state's hash to use as
        // the index into the table. Determines the size of the table.
        // 26 gives a 1 GB table. Must at least as big as indexBits.
        private const int hashIndexBits = 26;

        // The number of bits of a state's hash to store in the index.
        // Do not change this number.
        private const int indexBits = 19;

        public const int TableSize = 2 * 60000049;
        public const long MemorySpaceBytes = sizeof(ulong) * (long)TableSize;

        private long size = 0;
        public long Size { get { return size; } }

        private long insertions = 0;
        public long Insertions { get { return insertions; } }

        private long requests = 0;
        public long Requests { get { return requests; } }

        private long collisions = 0;
        public long Collisions { get { return collisions; } }

        public void Add(int move, int bestMove, ulong hash, int score,
            int nodeType)
        {
            insertions++;

            ulong entry = CreateEntry(move, bestMove, hash, score, nodeType);

            // The index is the hashIndexBits most significant bits and a zero
            // as the least significant bit to distinguish between the always and
            // depth entries.
            int index = 2 * (int)(hash % (TableSize / 2));// (int)((hash >> (64 - hashIndexBits)) << 1);

            ulong currentEntry = table[index];

            // Use the depth entry if it is empty or the given entry is
            // larger.
            if (currentEntry == 0)
            {
                size++;
                table[index] = entry;
            }

            else if (GetMove(currentEntry) < move)
            {
                collisions++;
                table[index + 1] = currentEntry;
                table[index] = entry;
            }

            // Otherwise, use the always entry.
            else if (table[index + 1] == 0)
            {
                size++;
                table[index + 1] = entry;
            }

            else
            {
                collisions++;
                table[index + 1] = entry;
            }
        }

        public bool Lookup(ulong hash, out ulong result)
        {
            requests++;

            int index = 2 * (int)(hash % (TableSize / 2));// (int)((hash >> (64 - hashIndexBits)) << 1);(int)((hash >> (64 - hashIndexBits)) << 1);
            ulong maskedHash = hash & (((ulong)1 << (64 - indexBits)) - 1);

            result = table[index];
            if (result != 0 && (result >> indexBits) == maskedHash)
            {
                return true;
            }

            result = table[index + 1];
            return result != 0 && (result >> indexBits) == maskedHash;
        }

        public void ResetStatistics()
        {
            insertions = 0;
            requests = 0;
            collisions = 0;
        }

        // Returns a ulong organised as follows:
        // Field: |-Move-|-NodeType-|-Score-|-BestMove-|-Hash (bits 0 to 44)-|
        // Bit:   |0----5|6--------7|8----15|16------18|19-----------------63|
        public static ulong CreateEntry(int move, int bestMove, ulong hash, int score,
            int nodeType)
        {
            Debug.Assert((0 <= move && move <= 7 * 6) || move == 63);
            Debug.Assert(0 <= bestMove && bestMove <= 6);
            Debug.Assert(-128 <= score && score <= 127);
            Debug.Assert(1 <= nodeType && nodeType <= 3);

            ulong result = (ulong)move;
            result |= (ulong)nodeType << 6;
            result |= (ulong)(score + 128) << 8;
            result |= (ulong)bestMove << 16;
            result |= hash << indexBits; // This erases the 19 most significant bits, but
                                         // they can be retrived from the index instead.

            return result;
        }

        public static int GetMove(ulong data)
        {
            return (int)(data & 0x3F);
        }

        public static int GetNodeType(ulong data)
        {
            return (int)((data >> 6) & 0x3);
        }

        public static int GetScore(ulong data)
        {
            return (int)(((data >> 8) & 0xFF) - 128);
        }

        public static int GetBestMove(ulong data)
        {
            return (int)((data >> 16) & 0x7);
        }

        public static ulong GetHash(ulong data)
        {
            return data >> 19;
        }
    }
}
