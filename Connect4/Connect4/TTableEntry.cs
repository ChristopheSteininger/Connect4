using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    enum NodeType { Exact, Upper, Lower }

    class TTableEntry
    {
        private Grid state;
        public Grid State
        {
            get { return state; }
        }

        private int depth;
        public int Depth
        {
            get { return depth; }
        }

        private int score;
        public int Score
        {
            get { return score; }
        }

        private NodeType nodeType;
        public NodeType NodeType
        {
            get { return nodeType; }
        }

        private TTableEntry next;
        public TTableEntry Next
        {
            get { return next; }
        }

        public TTableEntry(Grid state, int depth, int score, NodeType nodeType)
        {
            this.state = state;
            this.depth = depth;
            this.score = score;
            this.nodeType = nodeType;
            this.next = null;
        }

        public void LinkTo(TTableEntry entry)
        {
            next = entry;
        }
    }
}
