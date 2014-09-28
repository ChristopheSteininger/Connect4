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

        // A three dimensional table of arrays of masks, accessed by row, column then count.
        private ulong[][] lazyMasks;

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

        public void SetLastMove(int lastMove)
        {
            this.lastMove = lastMove;
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

            int row = nextFreeTile[lastMove] - 1;
            ulong[] masks = lazyMasks[row + (lastMove * height) + (width * height)];
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

        public bool LazyIsGameOverAndIsValidMove(int player, int move)
        {
            if (nextFreeTile[move] < height)
            {
                int row = nextFreeTile[move];
                ulong[] masks = lazyMasks[row + (move * height) + (width * height)];
                ulong playerPosition = playerPositions[player]
                    | (ulong)1 << (move + row * width);

                for (int i = 0; i < masks.Length; i++)
                {
                    if ((playerPosition & masks[i]) == masks[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public int Evaluate(int player)
        {
            ulong[] winningRows = streakMasks[0];
            ulong playerPosition = playerPositions[player];
            ulong opposingPosition = playerPositions[1 - player];

            int score = 0;
            for (int i = 0; i < winningRows.Length; i++)
            {
                bool playerOnRow = (playerPosition & winningRows[i]) != 0;
                bool oppositionOnRow = (opposingPosition & winningRows[i]) != 0;

                // Increment the score if the player has at least one piece on
                // the row, but the opposing player does not.
                if (playerOnRow && !oppositionOnRow)
                {
                    score++;
                }

                // Otherwise, decrement the score if this row belongs to the
                // opposing player.
                else if (!playerOnRow && oppositionOnRow)
                {
                    score--;
                }
            }

            return score;
        }

        public int CountThreats(int player)
        {
            ulong[] winningRows = streakMasks[1];
            ulong p1Position = playerPositions[0];
            ulong p2Position = playerPositions[1];
            const ulong rowMask = 0x7F01FC07F; // Contains a 1 in all odd position.
            //const ulong rowMask = 0x7F01FC000; // Contains a 1 in all odd position.

            int p1WinningRows = 0;
            int p2WinningRows = 0;
            int p1OddThreats = 0;
            int p1EvenThreats = 0;
            int p2OddThreats = 0;
            int p2EvenThreats = 0;
            //int[][] allWeights = new int[][] {
            //    new int[] { 1, -1, 10, -10, 10, -10 },
            //    new int[] { -1, 1, -10, 10, -10, 10 }
            //};
            int[][] allWeights = new int[][] {
                new int[] { 1, -1, 0, 0, 0, 0 },
                new int[] { -1, 1, 0, 0, 0, 0 }
            };

            for (int i = 0; i < winningRows.Length; i++)
            {
                // Find which squares each player needs to win with this row.
                ulong p1MissingSquares = winningRows[i] & ~p1Position;
                ulong p2MissingSquares = winningRows[i] & ~p2Position;

                // This is a threat only if one square is missing and that square
                // is empty.
                if ((p1MissingSquares & (p1MissingSquares - 1)) == 0
                    && (p1MissingSquares & p2Position) == 0)
                {
                    // If the threat is on an odd row.
                    if ((p1MissingSquares & rowMask) == p1MissingSquares)
                    {
                        p1OddThreats++;
                    }
                    else
                    {
                        p1EvenThreats++;
                    }
                }

                else if ((p2MissingSquares & (p2MissingSquares - 1)) == 0
                    && (p2MissingSquares & p1Position) == 0)
                {
                    // If the threat is on an odd row.
                    if ((p1MissingSquares & rowMask) == p1MissingSquares)
                    {
                        p2OddThreats++;
                    }
                    else
                    {
                        p2EvenThreats++;
                    }
                }

                bool p1OnRow = (p1Position & winningRows[i]) != 0;
                bool p2OnRow = (p2Position & winningRows[i]) != 0;

                // Increment the score if the player has at least one piece on
                // the row, but the opposing player does not.
                if (p1OnRow && !p2OnRow)
                {
                    p1WinningRows++;
                }

                // Otherwise, decrement the score if this row belongs to the
                // opposing player.
                else if (!p1OnRow && p2OnRow)
                {
                    p2WinningRows++;
                }
            }

            int[] weights = allWeights[player];

            return weights[0] * p1WinningRows + weights[1] * p2WinningRows
                + weights[2] * p1OddThreats + weights[3] * p2OddThreats
                + weights[4] * p1EvenThreats + weights[5] * p2EvenThreats;
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
            int row = nextFreeTile[move] - 1;
            ulong[] masks = lazyMasks[row + (move * height) + 0];
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
            lazyMasks = new ulong[height * width * 2][];
            for (int i = 0; i < lazyMasks.Length; i++)
            {
                int row = i % height;
                int column = (i / height) % width;
                int length = i / (height * width);
                lazyMasks[i] = tempLazyMasks[length, row, column].ToArray();
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
                    //tempMasks.Add(verticalMask);
                    tempLazyMasks[length - 3, y + length - 1, x].Add(verticalMask);

                    verticalMask <<= 1;
                }
            }
        }
    }
}
