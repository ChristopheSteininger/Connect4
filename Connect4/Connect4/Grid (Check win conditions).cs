using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    partial class Grid
    {
        private ulong[] gameOverMasks;

        /// <summary>
        /// Returns a value indicating which player, if any, won the game.
        /// </summary>
        /// <returns>-1 if the game is not over, otherwise 0 if player 1 won or 1 if player 2
        /// won.</returns>
        public int IsGameOver()
        {
            // Each mask is a possible way to win a game. If a mask is in a player's position
            // array, then the player has won.
            for (int player = 0; player < 2; player++)
            {
                for (int i = 0; i < gameOverMasks.Length; i++)
                {
                    if ((playerPositions[player] & gameOverMasks[i]) == gameOverMasks[i])
                    {
                        return player;
                    }
                }
            }

            return -1;
        }

        private void SetGameOverMasks()
        {
            List<ulong> masks = new List<ulong>();

            // Create the horizontal masks.
            ulong horizontalMask = 1 + 2 + 4 + 8;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x <= width - 4; x++)
                {
                    masks.Add(horizontalMask);
                    horizontalMask <<= 1;
                }

                horizontalMask <<= 3;
            }

            // Create the vertical masks.
            ulong verticalMask = 1;
            verticalMask = (verticalMask << width) + 1;
            verticalMask = (verticalMask << width) + 1;
            verticalMask = (verticalMask << width) + 1;

            for (int y = 0; y <= height - 4; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    masks.Add(verticalMask);
                    verticalMask <<= 1;
                }
            }

            // Create diagonal masks.
            ulong negativeDiagonalMask = 1 << 3;
            negativeDiagonalMask += (ulong)1 << (2 + width);
            negativeDiagonalMask += (ulong)1 << (1 + width * 2);
            negativeDiagonalMask += (ulong)1 << (0 + width * 3);

            ulong positiveDiagonalMask = 1;
            positiveDiagonalMask += (ulong)1 << (1 + width);
            positiveDiagonalMask += (ulong)1 << (2 + width * 2);
            positiveDiagonalMask += (ulong)1 << (3 + width * 3);

            for (int y = 0; y <= height - 4; y++)
            {
                for (int x = 0; x <= width - 4; x++)
                {
                    masks.Add(negativeDiagonalMask);
                    negativeDiagonalMask <<= 1;

                    masks.Add(positiveDiagonalMask);
                    positiveDiagonalMask <<= 1;
                }

                negativeDiagonalMask <<= 3;
                positiveDiagonalMask <<= 3;
            }

            gameOverMasks = masks.ToArray();
        }
    }
}
