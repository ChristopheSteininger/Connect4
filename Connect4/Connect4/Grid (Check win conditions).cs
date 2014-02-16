using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Connect4
{
    partial class Grid
    {
        // Two arrays of masks. The first is an array of all possible streaks of 3 pieces,
        // the second of 4 pieces.
        private ulong[][] streakMasks;

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
                for (int i = 0; i < streakMasks[1].Length; i++)
                {
                    if ((playerPositions[player] & streakMasks[1][i]) == streakMasks[1][i])
                    {
                        return player;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Similar to IsGameOver() but only checks one player.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns></returns>
        public int IsGameOver(int player)
        {
            ulong[] gameOverMasks = streakMasks[1];
            ulong playerPosition = playerPositions[player];

            for (int i = 0; i < gameOverMasks.Length; i++)
            {
                if ((playerPosition & gameOverMasks[i]) == gameOverMasks[i])
                {
                    return player;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the number of 3 piece streaks which the player has.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>The number of 3 piece streaks</returns>
        public int GetPlayerStreaks(int player)
        {
            Debug.Assert(player == 0 || player == 1);

            int result = 0;
            ulong[] currentStreakMask = streakMasks[0];

            for (int i = 0; i < currentStreakMask.Length; i++)
            {
                if ((playerPositions[player] & currentStreakMask[i]) == currentStreakMask[i])
                {
                    result++;
                }
            }

            return result;
        }

        private void SetStreakMasks()
        {
            streakMasks = new ulong[2][];

            for (int length = 3; length <= 4; length++)
            {
                List<ulong> masks = new List<ulong>();

                AddHorizontalMasks(masks, length);
                AddVerticalMasks(masks, length);
                AddDiagonalMasks(masks, length);

                streakMasks[length - 3] = masks.ToArray();
            }
        }

        private void AddDiagonalMasks(List<ulong> masks, int length)
        {
            Debug.Assert(2 <= length && length <= 4);

            ulong negativeDiagonalMask = 0;
            ulong positiveDiagonalMask = 0;

            for (int i = 0; i < length; i++)
            {
                negativeDiagonalMask += (ulong)1 << ((length - i - 1) + width * i);
                positiveDiagonalMask += (ulong)1 << (i + width * i);
            }

            for (int y = 0; y <= height - length; y++)
            {
                for (int x = 0; x <= width - length; x++)
                {
                    masks.Add(negativeDiagonalMask);
                    negativeDiagonalMask <<= 1;

                    masks.Add(positiveDiagonalMask);
                    positiveDiagonalMask <<= 1;
                }

                negativeDiagonalMask <<= 3;
                positiveDiagonalMask <<= 3;
            }
        }

        private void AddHorizontalMasks(List<ulong> masks, int length)
        {
            Debug.Assert(2 <= length && length <= 4);

            ulong horizontalMask = ((ulong)1 << length) - 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x <= width - length; x++)
                {
                    masks.Add(horizontalMask);
                    horizontalMask <<= 1;
                }

                horizontalMask <<= 3;
            }
        }

        private void AddVerticalMasks(List<ulong> masks, int length)
        {
            Debug.Assert(2 <= length && length <= 4);

            ulong verticalMask = 1;

            for (int i = 0; i < length - 1; i++)
            {
                verticalMask = (verticalMask << width) + 1;
            }

            for (int y = 0; y <= height - length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    masks.Add(verticalMask);
                    verticalMask <<= 1;
                }
            }
        }
    }
}
