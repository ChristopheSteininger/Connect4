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
        private bool maximise;

        private int score;
        public int Score
        {
            get { return score; }
        }

        private MinimaxNode bestChild;
        public MinimaxNode BestChild
        {
            get { return bestChild; }
        }

        private int bestMove;
        public int BestMove
        {
            get { return bestMove; }
        }

        private int move;
        public int Move
        {
            get { return move; }
        }

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
            this.score = (maximise ? int.MinValue : int.MaxValue);
            this.maximise = maximise;
        }

        public void AddChild(MinimaxNode child, int move)
        {
            child.move = move;
            children.Add(child);

            if (maximise && child.score > score
                || !maximise && child.score < score)
            {
                score = child.score;
                bestMove = move;
                bestChild = child;
            }
        }

        public MinimaxNode GetChild(int index)
        {
            Debug.Assert(0 <= index && index < children.Count);

            return children[index];
        }

        public List<MinimaxNode> GetChildren()
        {
            return new List<MinimaxNode>(children);
        }
    }
}
