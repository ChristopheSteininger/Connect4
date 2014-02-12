using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class AIPlayer : Player
    {
        private readonly int moveLookAhead = 9;
        private MinimaxNode root;

        private MouseState oldMouseState;

        // Statistics for console.
        private bool printToConsole = true;
        private int totalNodesSearched;
        private int endNodesSearched;

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
            Console.Title = "Debugging output for AI player";
            Console.SetWindowSize(84, 60);

            if (printToConsole)
            {
                totalNodesSearched = 0;
                endNodesSearched = 0;
                Console.Write("Calculating next move . . . ");
            }

            // Get the best move and measure the runtime.
            DateTime startTime = DateTime.Now;
            root = Minimax(moveLookAhead, grid, player, -MinimaxNode.Infinity,
                MinimaxNode.Infinity);
            double runtime = (DateTime.Now - startTime).TotalMilliseconds;

            PrintMoveStatistics(runtime);

            oldMouseState = Mouse.GetState();

            return root.BestMove;
        }

        private void PrintMoveStatistics(double runtime)
        {
            if (printToConsole)
            {
                // Print the number of nodes looked at and the search time.
                double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
                Console.WriteLine("Done");
                Console.WriteLine("Analysed {0} nodes, including {1} end nodes in "
                    + "{2}ms ({3}nodes/ms).", totalNodesSearched,
                    endNodesSearched, runtime, nodesPerMillisecond);

                // Scores the scores of the child and grandchild states.
                Console.WriteLine("Move scores (Move[Score](Grandchildren):");
                foreach (MinimaxNode child in root.GetChildren())
                {
                    // Highlight the move taken by the computer.
                    if (child == root.BestChild)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    // Print the grandchildren.
                    Console.Write("\t{0}[{1}](", child.Move, child.Score);
                    foreach (MinimaxNode grandchild in child.GetChildren())
                    {
                        Console.Write("{0}[{1}] ", grandchild.Move, grandchild.Score);
                    }
                    Console.WriteLine("\b) ");

                    Console.ResetColor();
                }

                Console.WriteLine("(Best opponent move is {0}).",
                    root.BestChild.BestMove);

                Console.WriteLine();
            }
        }

        private MinimaxNode Minimax(int depth, Grid state, int currentPlayer,
            int alpha, int beta)
        {
            Debug.Assert(depth >= 0);

            totalNodesSearched++;

            int gameOverResult = state.IsGameOver();

            // Return the maximum value if the player won the game.
            if (gameOverResult == player)
            {
                endNodesSearched++;
                return new MinimaxNode(MinimaxNode.Infinity);
            }

            // Return the minimum value if the opposing player won the game.
            if (gameOverResult == 1 - player)
            {
                endNodesSearched++;
                return new MinimaxNode(-MinimaxNode.Infinity);
            }

            // Evaluate the state if this is a terminal state.
            if (depth == 0)
            {
                return new MinimaxNode(EvaluateState(state));
            }

            // Otherwise, find the best move.
            MinimaxNode result = new MinimaxNode(currentPlayer == player);
            for (int move = 0; move < state.Width; move++)
            {
                if (state.IsValidMove(move))
                {
                    MinimaxNode child = Minimax(depth - 1, state.Move(move, currentPlayer),
                        1 - currentPlayer, alpha, beta);
                    result.AddChild(child, move);

                    // Update alpha and beta values.
                    if (currentPlayer == player)
                    {
                        alpha = result.Score;
                    }
                    else
                    {
                        beta = result.Score;
                    }

                    // Possibly return early.
                    if (beta <= alpha)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        private int EvaluateState(Grid state)
        {
            // Otherwise, guess how good the state is.
            int[] streaks = state.GetPlayerStreaks(player);
            int heuristic = streaks[0] + 3 * streaks[1] + 5 * streaks[2];

            return heuristic;
        }
    }
}
