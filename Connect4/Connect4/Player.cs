using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Connect4
{
    class Player
    {
        private int player;
        private const int moveLookAhead = 5;

        public Player(int player)
        {
            this.player = player;
        }

        public int GetMove(Grid grid)
        {
            Logger.Log("New move");
            Logger.Log(grid.ToString());

            int bestMove = 0;
            Minimax(moveLookAhead, grid, player, ref bestMove, true);

            return bestMove;
        }

        private int Minimax(int depth, Grid state, int currentPlayer,
            ref int bestMove, bool setBestMove)
        {
            Debug.Assert(depth >= 0);

            Logger.Log("At depth " + depth);
            Logger.EnterNewLevel();

            int plusInfinity = int.MaxValue;
            int minusInfinity = int.MinValue;

            // Return plus or minus infinity if the game is over.
            int gameOver = state.IsGameOver();
            if (gameOver > 0)
            {
                if (gameOver == player)
                {
                    return plusInfinity;
                }
                else
                {
                    return minusInfinity;
                }
            }

            // Return the heuristic if the maximum depth has been reached.
            if (depth == 0)
            {
                int[] streaks = state.GetPlayerStreaks(player);
                int heuristic = streaks[0] + 3 * streaks[1] + 5 * streaks[2];

                Logger.Log("Streaks: 0:" + streaks[0] + " 1:" + streaks[1] + " 2:" + streaks[2]);
                Logger.Log("Returning " + heuristic);
                Logger.ExitLevel();

                return heuristic;
            }

            // Otherwise recurse.
            int nextPlayer = 1 - currentPlayer;
            int best = currentPlayer == player ? minusInfinity : plusInfinity;
            int result = 0;
            for (int i = 0; i < state.Size; i++)
            {
                if (state.IsValidMove(i))
                {
                    Grid nextMove = state.Move(i, currentPlayer);

                    Logger.Log("Recursing with:");
                    Logger.Log(nextMove.ToString());
                    result = Minimax(depth - 1, nextMove, nextPlayer,
                        ref bestMove, false);

                    if (currentPlayer == player && result > best)
                    {
                        best = result;
                        if (setBestMove)
                        {
                            bestMove = i;
                        }
                    }

                    else if (currentPlayer != player && result < best)
                    {
                        best = result;
                        if (setBestMove)
                        {
                            bestMove = i;
                        }
                    }
                }
            }

            Logger.Log("Depth " + depth + ", returning " + best);
            Logger.ExitLevel();

            return best;
        }
    }
}
