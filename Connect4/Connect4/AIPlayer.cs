using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class AIPlayer : Player
    {
        private readonly int moveLookAhead = 5;
        private MinimaxNode root;

        private MouseState oldMouseState;

        public AIPlayer(int player)
            : base(player)
        {
        }

        public AIPlayer(int player, int moveLookAhead)
            : base(player)
        {
            this.moveLookAhead = moveLookAhead;
        }

        public override int GetMove(Grid grid)
        {
            int bestMove = -1;
            if (true)//Mouse.GetState().LeftButton == ButtonState.Pressed
                //&& oldMouseState.LeftButton == ButtonState.Released)
            {
                Logger.Log("New move");
                Logger.Log(grid.ToString());

                root = Minimax(moveLookAhead, grid, player);
                bestMove = root.GetBestMove();
            }

            oldMouseState = Mouse.GetState();

            return bestMove;
        }

        private MinimaxNode Minimax(int depth, Grid state, int currentPlayer)
        {
            Debug.Assert(depth >= 0);

            // Evaluate the state if this is a terminal state.
            int gameOverResult = state.IsGameOver();
            if (depth == 0 || gameOverResult != -1)
            {
                int score = EvaluateState(state, gameOverResult, currentPlayer);
                if (currentPlayer != player)
                {
                    score *= -1;
                }

                return new MinimaxNode(state, score);
            }

            // Otherwise, find the best move.
            MinimaxNode result = new MinimaxNode(state, currentPlayer == player);
            for (int move = 0; move < state.Size; move++)
            {
                if (state.IsValidMove(move))
                {
                    result.AddChild(Minimax(depth - 1, state.Move(move, currentPlayer),
                        1 - currentPlayer), move);
                }
            }

            return result;
        }

        private int EvaluateState(Grid state, int gameOverResult, int player)
        {
            // Return the maximum value if the player won the game.
            if (gameOverResult == player)
            {
                return MinimaxNode.Infinity;
            }

            // Return the minimum value if the opposing player won the game.
            if (gameOverResult == 1 - player)
            {
                return -MinimaxNode.Infinity;
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
