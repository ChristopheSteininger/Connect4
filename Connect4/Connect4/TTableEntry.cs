using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    enum NodeType { Exact, Upper, Lower }

    class TTableEntry
    {
        public readonly Grid State;
        public readonly int Depth;
        public readonly ulong Hash;
        public readonly int Score;
        public readonly NodeType NodeType;

        private TTableEntry next;
        public TTableEntry Next
        {
            get { return next; }
        }

        public TTableEntry(Grid state, int depth, int score, NodeType nodeType)
        {
            this.State = state;
            this.Depth = depth;
            this.Hash = state.GetTTableHash();
            this.Score = score;
            this.NodeType = nodeType;
            this.next = null;
        }

        public void LinkTo(TTableEntry entry)
        {
            next = entry;
        }
    }
}
