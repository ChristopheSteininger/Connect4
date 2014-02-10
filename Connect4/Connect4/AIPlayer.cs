using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Connect4
{
    class AIPlayer : Player
    {
        private const int moveLookAhead = 5;

        public AIPlayer(int player)
            : base(player)
        {
        }

        public override int GetMove(Grid grid)
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

            // Evaluate the state if this is a terminal state.
            int gameOverResult = state.IsGameOver();
            if (depth == 0 || gameOverResult != -1)
            {
                return EvaluateState(state, gameOverResult, player);
            }

            // Mark the bestMove to be set to the first valid move.
            if (setBestMove)
            {
                bestMove = -1;
            }

            // Otherwise, find the best move.
            int best = int.MinValue + 1;
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

            // Evaluate the state if this is a terminal state.
            int gameOverResult = state.IsGameOver();
            if (depth == 0 || gameOverResult != -1)
            {
                return -EvaluateState(state, gameOverResult, 1 - player);
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

        private int EvaluateState(Grid state, int gameOverResult, int player)
        {
            // Return the maximum value if the player won the game.
            if (gameOverResult == player)
            {
                return int.MaxValue;
            }

            // Return the minimum value if the opposing player won the game.
            if (gameOverResult == 1 - player)
            {
                return int.MinValue + 1;
            }

            // Otherwise, guess how good the state is.
            int[] streaks = state.GetPlayerStreaks(player);
            int heuristic = streaks[0] + 3 * streaks[1] + 5 * streaks[2];

            Logger.Log("Streaks: 0:" + streaks[0] + " 1:" + streaks[1] + " 2:" + streaks[2]);
            Logger.Log("Returning " + heuristic);
            Logger.ExitLevel();

            return heuristic;
        }
    }
}
