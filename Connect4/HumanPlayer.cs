using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    class HumanPlayer : Player
    {
        private int highlightedColumn = -1;

        public HumanPlayer(int player)
            : base(player)
        {
        }

        public override int GetMove(Grid grid)
        {
            int move = -1;

            //if (Mouse.GetState().LeftButton == ButtonState.Pressed
            //    && oldMouseState.LeftButton == ButtonState.Released)
            //{
            //    move = highlightedColumn;
            //}

            //oldMouseState = Mouse.GetState();

            return move;
        }

        public override int HighlightColumn(int startX, int startY, int endY,
            int columnSize)
        {
            //MouseState mouseState = Mouse.GetState();

            //if (startY <= mouseState.Y && mouseState.Y <= endY
            //    && mouseState.X >= startX)
            //{
            //    highlightedColumn = (Mouse.GetState().X - startX) / columnSize;
            //}
            //else
            //{
            //    highlightedColumn = -1;
            //}

            return highlightedColumn;
        }
    }
}
