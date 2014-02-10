using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class HumanPlayer : Player
    {
        private KeyboardState oldKeyboardState;
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

            // Only allow one key to be pressed.
            Keys[] pressedKeys = Keyboard.GetState().GetPressedKeys();
            if (pressedKeys.Length != 0 && oldKeyboardState.IsKeyUp(pressedKeys[0]))
            {
                move = numberKeys.IndexOf(pressedKeys[0]);
            }

            oldKeyboardState = Keyboard.GetState();

            return move;
        }
    }
}
