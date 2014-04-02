using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    class TranspositionTable
    {
        public const int TableSize = 8306069;
        public const int SearchSize = 3;

        private TTableEntry[] table = new TTableEntry[TableSize];

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

        public TranspositionTable()
        {
            // Initialise the table, a depth of -1 means free entry.
            for (int i = 0; i < TableSize; i++)
            {
                table[i] = new TTableEntry(-1, 0, 0, 0, 0);
            }
        }

        public void Add(TTableEntry entry)
        {
            insertions++;

            ulong entryHash = entry.Hash;

            int bestIndex = -1;
            int minDepth = -1;

            // Check up to searchSize entries to find the entry with the
            // lowest depth.
            for (ulong i = 0; i < SearchSize; i++)
            {
                int index = (int)((entryHash + i) % TableSize);
                TTableEntry currentEntry = table[index];

                // If an entry is null, take it regardless of the depth
                // of other entries.
                if (currentEntry.Depth == -1)
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

            ulong i = 0;
            do
            {
                int index = (int)((stateHash + i) % TableSize);
                result = table[index];

                if (result.Depth != -1 && result.Hash == stateHash)
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
    }
}
