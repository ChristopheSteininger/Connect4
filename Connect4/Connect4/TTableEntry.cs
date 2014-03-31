using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    enum NodeType { Exact, Upper, Lower }

    class TTableEntry
    {
        public readonly int Depth;
        public readonly int BestMove;
        public readonly ulong Hash;
        public readonly int Score;
        public readonly NodeType NodeType;

        public TTableEntry(int depth, int bestMove, ulong hash, int score, 
            NodeType nodeType)
        {
            this.Depth = depth;
            this.BestMove = bestMove;
            this.Hash = hash;
            this.Score = score;
            this.NodeType = nodeType;
        }
    }
}
