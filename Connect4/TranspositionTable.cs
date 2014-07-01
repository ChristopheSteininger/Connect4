using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    class TranspositionTable
    {
        public const int TableSize = 1 << (hashIndexBits + 1);

        // The number of most significant bits of a state's hash to use as
        // the index into the table. Do not mess around with this constant.
        private const int hashIndexBits = 22;

        private ulong[] table = new ulong[TableSize];

        private int size = 0;
        public int Size
        {
            get { return size; }
        }

        private int insertions = 0;
        public int Insertions
        {
            get { return insertions; }
        }

        private int requests = 0;
        public int Requests
        {
            get { return requests; }
        }

        private int collisions = 0;
        public int Collisions
        {
            get { return collisions; }
        }

        public void Add(int depth, int bestMove, ulong hash, int score,
            int nodeType)
        {
            insertions++;

            ulong entry = CreateEntry(depth, bestMove, hash, score, nodeType);

            // The index is the hashIndexBits most significant bits and a zero
            // as the least significant bit to distinguish between the always and
            // depth entries.
            int index = (int)((hash >> (64 - hashIndexBits)) << 1);

            ulong currentEntry = table[index];

            // Use the depth entry if it is empty or the given entry is
            // larger.
            if (currentEntry == 0)
            {
                size++;
                table[index] = entry;
            }

            else if (GetDepth(currentEntry) < depth)
            {
                collisions++;
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

        public bool TryGet(Grid state, out ulong result)
        {
            requests++;

            int index = (int)((state.Hash >> (64 - hashIndexBits)) << 1);
            ulong maskedHash = state.Hash & (((ulong)1 << 45) - 1);

            result = table[index];
            if (result != 0 && (result >> 19) == maskedHash)
            {
                return true;
            }

            result = table[index + 1];
            return result != 0 && (result >> 19) == maskedHash;
        }

        public void ResetStatistics()
        {
            insertions = 0;
            requests = 0;
            collisions = 0;
        }

        // Returns a ulong organised as follows:
        // Field: |-Depth-|-NodeType-|-Score-|-BestMove-|-Hash (bits 0 to 44)-|
        // Bit:   |0-----5|6--------7|8----15|16------18|19-----------------63|
        public static ulong CreateEntry(int depth, int bestMove, ulong hash, int score,
            int nodeType)
        {
            Debug.Assert((0 <= depth && depth <= 7 * 6) || depth == 63);
            Debug.Assert(0 <= bestMove && bestMove <= 6);
            Debug.Assert(-128 <= score && score <= 127);
            Debug.Assert(1 <= nodeType && nodeType <= 3);

            ulong result = (ulong)depth;
            result |= (ulong)nodeType << 6;
            result |= (ulong)(score + 128) << 8;
            result |= (ulong)bestMove << 16;
            result |= hash << 19; // This erases the 19 most significant bits, but
                                  // they can be retrived from the index instead.

            return result;
        }

        public static int GetDepth(ulong data)
        {
            return (int)(data & ((1 << 6) - 1));
        }

        public static int GetNodeType(ulong data)
        {
            return (int)((data >> 6) & ((1 << 2) - 1));
        }

        public static int GetScore(ulong data)
        {
            return (int)((data >> 8) & ((1 << 8) - 1)) - 128;
        }

        public static int GetBestMove(ulong data)
        {
            return (int)((data >> 16) & ((1 << 3) - 1));
        }

        public static ulong GetHash(ulong data)
        {
            return data >> 19;
        }
    }
}
