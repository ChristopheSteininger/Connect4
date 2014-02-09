using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Connect4
{
    class Player
    {
        private int player;
        private const int moveLookAhead = 7;

        public Player(int player)
        {
            this.player = player;
        }

        public int GetMove(Grid grid)
        {
            Logger.Log("New move");
            Logger.Log(grid.ToString());

            int bestMove = 0;
            Maximise(moveLookAhead, grid, true, ref bestMove);

            return bestMove;
        }

        private int Maximise(int depth, Grid state, bool setBestMove,
            ref int bestMove)
        {
            Debug.Assert(depth >= 0);

            // Return the maximum value if the player won the game.
            if (state.IsGameOver() == player)
            {
                return int.MaxValue;
            }

            // Guess how good the state is if the maximum depth has
            // been reached.
            if (depth == 0)
            {
                return Heuristic(state, player);
            }

            // Mark the bestMove to be set to the first valid move.
            if (setBestMove)
            {
                bestMove = -1;
            }

            // Otherwise, find the best move.
            int best = int.MinValue;
            for (int move = 0; move < state.Size; move++)
            {
                if (state.IsValidMove(move))
                {
                    int result = Minimise(depth - 1, state.Move(move, player),
                        false, ref bestMove);

                    if (result > best)
                    {
                        best = result;
                        if (setBestMove)
                        {
                            bestMove = move;
                        }
                    }

                    if (setBestMove && bestMove == -1)
                    {
                        bestMove = move;
                    }
                }
            }

            return best;
        }

        private int Minimise(int depth, Grid state, bool setBestMove,
            ref int bestMove)
        {
            Debug.Assert(depth >= 0);

            // Return the minimum value if the opposing player won the game.
            if (state.IsGameOver() == 1 - player)
            {
                return int.MinValue;
            }

            // Guess how good the state is if the maximum depth has
            // been reached.
            if (depth == 0)
            {
                return Heuristic(state, 1 - player);
            }

            // Otherwise, find the worst move which the opposing player can
            // make.
            int worst = int.MaxValue;
            for (int move = 0; move < state.Size; move++)
            {
                if (state.IsValidMove(move))
                {
                    int result = Maximise(depth - 1, state.Move(move, 1 - player),
                        false, ref bestMove);
                    worst = Math.Min(result, worst);
                }
            }

            return worst;
        }

        private int Heuristic(Grid state, int player)
        {
            int[] streaks = state.GetPlayerStreaks(player);
            int heuristic = streaks[0] + 3 * streaks[1] + 5 * streaks[2];

            Logger.Log("Streaks: 0:" + streaks[0] + " 1:" + streaks[1] + " 2:" + streaks[2]);
            Logger.Log("Returning " + heuristic);
            Logger.ExitLevel();

            return heuristic;
        }
    }
}
