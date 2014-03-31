using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    class TranspositionTable
    {
        private const int tableSize = 1000009;
        private const int searchSize = 3;

        private TTableEntry[] table = new TTableEntry[tableSize];

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

        public void Add(TTableEntry entry)
        {
            insertions++;

            ulong entryHash = entry.Hash;

            int bestIndex = -1;
            int minDepth = -1;

            // Check up to searchSize entries to find the entry with the
            // lowest depth.
            for (ulong i = 0; i < searchSize; i++)
            {
                int index = (int)((entryHash + i) % tableSize);
                TTableEntry currentEntry = table[index];

                // If an entry is null, take it regardless of the depth
                // of other entries.
                if (currentEntry == null)
                {
                    table[index] = entry;
                    size++;

                    return;
                }

                int depth = currentEntry.Depth;
                if (i == 0 || minDepth > depth)
                {
                    bestIndex = index;
                    minDepth = depth;
                }
            }

            table[bestIndex] = entry;
            collisions++;
        }

        public bool TryGet(Grid state, out TTableEntry result)
        {
            requests++;

            ulong stateHash = state.GetTTableHash();

            for (ulong i = 0; i < searchSize; i++)
            {
                int index = (int)((stateHash + i) % tableSize);
                result = table[index];

                if (result != null && result.Hash == stateHash)
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        public void ResetStatistics()
        {
            insertions = 0;
            requests = 0;
            collisions = 0;
        }

        public void TestUsage(out double standardDeviation, out double averageBucketSize,
            out double averageFullBucketSize, out int fullBuckets)
        {
            int totalBucketSizeSquared = 0;

            fullBuckets = 0;

            for (int i = 0; i < tableSize; i++)
            {
                if (table[i] != null)
                {
                    fullBuckets++;
                }

                int bucketSize = 0;
                for (TTableEntry entry = table[i]; entry != null; entry = null/*entry.Next*/)
                {
                    bucketSize++;
                }

                totalBucketSizeSquared += bucketSize * bucketSize;
            }

            averageFullBucketSize = (double)size / fullBuckets;
            averageBucketSize = (double)size / tableSize;

            standardDeviation = Math.Sqrt(((double)totalBucketSizeSquared / tableSize)
                - averageBucketSize * averageBucketSize);

        }
    }
}
