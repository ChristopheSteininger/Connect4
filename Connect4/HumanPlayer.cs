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
            board.BoardPanel.MouseClick += new MouseEventHandler(panel_MouseClick);
        }

        void panel_MouseClick(object sender, MouseEventArgs e)
        {
            board.MoveWithHighlightedColumn(player);
        }
    }
}
