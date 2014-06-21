using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Connect4
{
    class HumanPlayer : Player
    {
        public HumanPlayer(int player, Board board)
            : base(player, board)
        {
        }

        public override void OnClick()
        {
            board.MoveWithHighlightedColumn(player);
        }
    }
}
