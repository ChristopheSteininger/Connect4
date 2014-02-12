﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Connect4
{
    partial class Grid
    {
        private struct CellStreaks
        {
            public int horizontal;
            public int vertical;
            public int positiveDiagonal;
            public int negativeDiagonal;
        }

        /// <summary>
        /// Returns a value indicating which player, if any, won the game.
        /// </summary>
        /// <returns>-1 if the game is not over, otherwise 0 if player 1 won or 1 if player 2
        /// won.</returns>
        public int IsGameOver()
        {
            int streakIndex = 0;
            CellStreaks[,] cellStreaks = new CellStreaks[2, width];

            // For each cell, look at the cell streaks of the cell to the left, bottom left,
            // bottom and and bottom right. If both cells are not empty and owned by the
            // same player, then use the neighbouring streak to set the streak struct of the
            // current cell.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Clear the current cell streaks.
                    cellStreaks[streakIndex, x].horizontal = 0;
                    cellStreaks[streakIndex, x].vertical = 0;
                    cellStreaks[streakIndex, x].positiveDiagonal = 0;
                    cellStreaks[streakIndex, x].negativeDiagonal = 0;

                    TileState currentState = GetTileState(y, x);

                    if (currentState != TileState.Empty)
                    {
                        // Check horizontal streaks.
                        if (x > 0 && currentState == GetTileState(y, x - 1))
                        {
                            cellStreaks[streakIndex, x].horizontal =
                                cellStreaks[streakIndex, x - 1].horizontal + 1;
                            if (cellStreaks[streakIndex, x].horizontal == 3)
                            {
                                return (int)currentState;
                            }
                        }

                        // Check vertical streaks.
                        if (y > 0 && currentState == GetTileState(y - 1, x))
                        {
                            cellStreaks[streakIndex, x].vertical =
                                cellStreaks[1 - streakIndex, x].vertical + 1;
                            if (cellStreaks[streakIndex, x].vertical == 3)
                            {
                                return (int)currentState;
                            }
                        }

                        // Check positive diagonal streaks.
                        if (x > 0 && y > 0
                            && currentState == GetTileState(y - 1, x - 1))
                        {
                            cellStreaks[streakIndex, x].positiveDiagonal =
                                cellStreaks[1 - streakIndex, x - 1].positiveDiagonal + 1;
                            if (cellStreaks[streakIndex, x].positiveDiagonal == 3)
                            {
                                return (int)currentState;
                            }
                        }

                        // Check negative diagonal streaks.
                        if (x < width - 1 && y > 0
                            && currentState == GetTileState(y - 1, x + 1))
                        {
                            cellStreaks[streakIndex, x].negativeDiagonal =
                                cellStreaks[1 - streakIndex, x + 1].negativeDiagonal + 1;
                            if (cellStreaks[streakIndex, x].negativeDiagonal == 3)
                            {
                                return (int)currentState;
                            }
                        }
                    }
                }

                // Swap which array is the bottom and which is current.
                streakIndex = 1 - streakIndex;
            }

            return -1;
        }
    }
}
