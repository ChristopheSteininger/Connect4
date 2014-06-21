using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    abstract class Player
    {
        protected readonly int player;
        protected readonly Board board;

        public Player(int player, Board board)
        {
            this.player = player;
            this.board = board;
        }

        public virtual void OnClick()
        {
        }

        public virtual void BeginMove()
        {
        }

        public virtual void GameOver(bool winner)
        {
        }
    }
}
