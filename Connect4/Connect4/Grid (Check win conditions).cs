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

        // Two arrays of masks used by the lazy evaluation and game over functions as above,
        // but each array points to a table of masks at each position of the board.
        private ulong[][][][] lazyMasks;

        private int lastMove = -1;

        // The flags for which players to update the streak count after a move.
        private bool[] updateLazyStreakCountForPlayer = new bool[] { false, false };
        public bool[] UpdateLazyStreakCountForPlayer
        {
            get { return updateLazyStreakCountForPlayer; }
            set { updateLazyStreakCountForPlayer = value; }
        }

        private int[] streakCount = new int[2] { 0, 0 };
        public int[] StreakCount
        {
            get { return streakCount; }
        }

        public void ClearMoveHistory()
        {
            lastMove = -1;
        }

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
        /// Similar to IsGameOver() but only checks one player and if the last move is part
        /// of the winning streak.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>True if the last move was a winning move for the player.</returns>
        public bool LazyIsGameOver(int player)
        {
            if (lastMove == -1)
            {
                return false;
            }

            ulong[] masks = lazyMasks[1][nextFreeTile[lastMove] - 1][lastMove];
            ulong playerPosition = playerPositions[player];

            for (int i = 0; i < masks.Length; i++)
            {
                if ((playerPosition & masks[i]) == masks[i])
                {
                    return true;
                }
            }

            return false;
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

        private void LazyUpdatePlayerStreaks(int player, int move, bool increase)
        {
            Debug.Assert(player == 0 || player == 1);

            if (!updateLazyStreakCountForPlayer[player])
            {
                return;
            }

            int result = 0;
            ulong[] masks = lazyMasks[0][nextFreeTile[move] - 1][move];
            ulong playerPosition = playerPositions[player];

            for (int i = 0; i < masks.Length; i++)
            {
                if ((playerPosition & masks[i]) == masks[i])
                {
                    result++;
                }
            }

            if (increase)
            {
                streakCount[player] += result;
            }
            else
            {
                streakCount[player] -= result;
            }
        }

        private void SetStreakMasks()
        {
            // Allocate the temporary lazy masks.
            List<ulong>[, ,] tempLazyMasks = new List<ulong>[2, height, width];
            for (int i = 0; i < 2; i++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        tempLazyMasks[i, y, x] = new List<ulong>();
                    }
                }
            }

            // Calculate the masks and store the results in the lists.
            streakMasks = new ulong[2][];
            List<ulong> tempMasks = new List<ulong>();
            for (int length = 3; length <= 4; length++)
            {
                tempMasks.Clear();

                AddHorizontalMasks(tempMasks, tempLazyMasks, length);
                AddVerticalMasks(tempMasks, tempLazyMasks, length);
                AddDiagonalMasks(tempMasks, tempLazyMasks, length);

                streakMasks[length - 3] = tempMasks.ToArray();
            }

            // Allocate the lazy masks array.
            lazyMasks = new ulong[2][][][];
            for (int i = 0; i < 2; i++)
            {
                lazyMasks[i] = new ulong[height][][];
                for (int y = 0; y < height; y++)
                {
                    lazyMasks[i][y] = new ulong[width][];
                    for (int x = 0; x < width; x++)
                    {
                        lazyMasks[i][y][x] = tempLazyMasks[i, y, x].ToArray();
                    }
                }
            }
        }

        private void AddDiagonalMasks(List<ulong> tempMasks,
            List<ulong>[, ,] tempLazyMasks, int length)
        {
            Debug.Assert(3 <= length && length <= 4);

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
                    tempMasks.Add(negativeDiagonalMask);
                    tempMasks.Add(positiveDiagonalMask);
                    for (int i = 0; i < length; i++)
                    {
                        tempLazyMasks[length - 3, y + i, x + i].Add(
                            positiveDiagonalMask);
                        tempLazyMasks[length - 3, y + i, x + length - 1 - i].Add(
                            negativeDiagonalMask);
                    }

                    negativeDiagonalMask <<= 1;
                    positiveDiagonalMask <<= 1;
                }

                negativeDiagonalMask <<= length - 1;
                positiveDiagonalMask <<= length - 1;
            }
        }

        private void AddHorizontalMasks(List<ulong> tempMasks,
            List<ulong>[, ,] tempLazyMasks, int length)
        {
            Debug.Assert(3 <= length && length <= 4);

            ulong horizontalMask = ((ulong)1 << length) - 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x <= width - length; x++)
                {
                    tempMasks.Add(horizontalMask);
                    for (int i = 0; i < length; i++)
                    {
                        tempLazyMasks[length - 3, y, x + i].Add(horizontalMask);
                    }

                    horizontalMask <<= 1;
                }

                horizontalMask <<= length - 1;
            }
        }

        private void AddVerticalMasks(List<ulong> tempMasks,
            List<ulong>[, ,] tempLazyMasks, int length)
        {
            Debug.Assert(3 <= length && length <= 4);

            ulong verticalMask = 1;

            for (int i = 0; i < length - 1; i++)
            {
                verticalMask = (verticalMask << width) + 1;
            }

            for (int y = 0; y <= height - length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tempMasks.Add(verticalMask);
                    tempLazyMasks[length - 3, y + length - 1, x].Add(verticalMask);

                    verticalMask <<= 1;
                }
            }
        }
    }
}
