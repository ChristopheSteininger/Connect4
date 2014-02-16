using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    class TranspositionTable
    {
        private const int tableSize = 500009;

        private TTableEntry[] table = new TTableEntry[tableSize];

        private int size = 0;
        public int Size
        {
            get { return size; }
        }

        private int collisions = 0;
        public int Collisions
        {
            get { return collisions; }
        }

        public void Add(TTableEntry entry)
        {
            int index = (int)(entry.State.GetTTableHash() % tableSize);

            if (table[index] == null)
            {
                table[index] = entry;
            }

            else
            {
                TTableEntry last;
                for (last = table[index]; last.Next != null; last = last.Next) ;

                last.LinkTo(entry);

                collisions++;
            }

            size++;
        }

        public bool TryGet(Grid state, out TTableEntry result)
        {
            int index = (int)(state.GetTTableHash() % tableSize);

            result = table[index];
            while (result != null)
            {
                if (state.Equals(result.State))
                {
                    return true;
                }

                result = result.Next;
            }

            return false;
        }
    }
}
