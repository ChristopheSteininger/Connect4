using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    class TranspositionTable
    {
        private const int tableSize = 1000009;

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
            int index = (int)(entry.Hash % tableSize);

            if (table[index] == null)
            {
                size++;

                table[index] = entry;
            }

            else if (table[index].Depth < entry.Depth)
            {
                table[index] = entry;

                collisions++;
            }

            insertions++;
        }

        public bool TryGet(Grid state, out TTableEntry result)
        {
            requests++;

            int index = (int)(state.GetTTableHash() % tableSize);

            result = table[index];
            return result != null && result.Hash == state.GetTTableHash();
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
                for (TTableEntry entry = table[i]; entry != null; entry = entry.Next)
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
