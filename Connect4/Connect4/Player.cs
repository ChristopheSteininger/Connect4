using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    abstract class Player
    {
        protected readonly int player;

        public Player(int player)
        {
            this.player = player;
        }

        public abstract int GetMove(Grid grid);
    }
}
