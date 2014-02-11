using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class HumanPlayer : Player
    {
        private int highlightedColumn = -1;

        private MouseState oldMouseState;
        private List<Keys> numberKeys = new List<Keys>();

        public HumanPlayer(int player)
            : base(player)
        {
            numberKeys.AddRange(new Keys[] { Keys.D1, Keys.D2, Keys.D3,
                    Keys.D4, Keys.D5, Keys.D6 });
        }

        public override int GetMove(Grid grid)
        {
            int move = -1;

            if (Mouse.GetState().LeftButton == ButtonState.Pressed
                && oldMouseState.LeftButton == ButtonState.Released)
            {
                move = highlightedColumn;
            }

            oldMouseState = Mouse.GetState();

            return move;
        }

        public override int HighlightColumn(int startX, int startY, int endY,
            int columnSize)
        {
            MouseState mouseState = Mouse.GetState();

            if (startY <= mouseState.Y && mouseState.Y <= endY)
            {
                highlightedColumn = (Mouse.GetState().X - startX) / columnSize;
            }
            else
            {
                highlightedColumn = -1;
            }

            return highlightedColumn;
        }
    }
}
