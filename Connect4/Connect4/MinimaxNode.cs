using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    class MinimaxNode
    {
        private Grid state;
        private int score;
        private bool maximise;

        private int bestMove;

        public const int Infinity = 1000000;

        private List<MinimaxNode> children = new List<MinimaxNode>();

        public MinimaxNode(Grid state, int score)
        {
            this.state = state;
            this.score = score;
        }

        public MinimaxNode(Grid state, bool maximise)
        {
            this.state = state;
            this.score = (maximise ? -Infinity : Infinity);
            this.maximise = maximise;
        }

        public void AddChild(MinimaxNode child, int move)
        {
            children.Add(child);

            if (maximise && child.score > score
                || !maximise && child.score < score)
            {
                score = child.score;
                bestMove = move;
            }
        }

        public MinimaxNode GetChild(int index)
        {
            Debug.Assert(0 <= index && index < children.Count);

            return children[index];
        }

        public int GetBestMove()
        {
            return bestMove;
        }
    }
}
