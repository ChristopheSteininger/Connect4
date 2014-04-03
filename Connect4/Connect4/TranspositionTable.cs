using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    class TranspositionTable
    {
        public const int TableSize = 8306069;
        public const int SearchSize = 1;

        // The number of most significant bits of a state's hash to use as
        // the index into the table.
        private const int hashIndexBits = 24;

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

            int bestIndex = 0;
            int minDepth = 0;

            ulong entry = CreateEntry(depth, bestMove, hash, score, nodeType);

            // Check up to searchSize entries to find the entry with the
            // lowest depth.
            for (ulong i = 0; i < SearchSize; i++)
            {
                int index = (int)(((hash >> (64 - hashIndexBits)) + i) % TableSize);
                ulong currentEntry = table[index];

                // If an entry is null, take it regardless of the depth
                // of other entries.
                if (currentEntry == 0)
                {
                    table[index] = entry;
                    size++;

                    return;
                }

                int currentDepth = GetDepth(currentEntry);
                if (i == 0 || minDepth > currentDepth)
                {
                    bestIndex = index;
                    minDepth = currentDepth;
                }
            }

            collisions++;

            table[bestIndex] = entry;
        }

        public bool TryGet(Grid state, out ulong result)
        {
            requests++;

            ulong hash = state.GetTTableHash() >> (64 - hashIndexBits);
            ulong maskedHash = state.GetTTableHash() & (((ulong)1 << 45) - 1);

            ulong i = 0;
            do
            {
                int index = (int)((hash + i) % TableSize);
                result = table[index];

                if (result != 0 && GetHash(result) == maskedHash)
                {
                    return true;
                }
                i++;
            } while (i < SearchSize);

            return false;
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
        public ulong CreateEntry(int depth, int bestMove, ulong hash, int score,
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
            result |= hash << 19;

            return result;
        }

        public int GetDepth(ulong data)
        {
            return (int)(data & ((1 << 6) - 1));
        }

        public int GetNodeType(ulong data)
        {
            return (int)((data >> 6) & ((1 << 2) - 1));
        }

        public int GetScore(ulong data)
        {
            return (int)((data >> 8) & ((1 << 8) - 1)) - 128;
        }

        public int GetBestMove(ulong data)
        {
            return (int)((data >> 16) & ((1 << 3) - 1));
        }

        public ulong GetHash(ulong data)
        {
            return data >> 19;
        }
    }
}
