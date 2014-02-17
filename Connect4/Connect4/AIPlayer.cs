using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class AIPlayer : Player
    {
        private const int infinity = 1000000;

        private readonly int moveLookAhead = 13;

        private TranspositionTable transpositionTable
            = new TranspositionTable();

        // Statistics for console.
        private bool printToConsole = true;
        private int totalNodesSearched;
        private int endNodesSearched;
        private int tableLookups;

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
                tableLookups = 0;
                Console.Write("Calculating next move . . . ");
            }

            int bestMove = -1;
            int score = -1;

            // Get the best move and measure the runtime.
            DateTime startTime = DateTime.Now;
            for (int depth = 1; depth < moveLookAhead; depth++)
            {
                score = Minimax(depth, grid, player, -infinity,
                    infinity, ref bestMove, true);
            }
            double runtime = (DateTime.Now - startTime).TotalMilliseconds;
            
            if (printToConsole)
            {
                PrintMoveStatistics(runtime, score);
            }

            return bestMove;
        }

        private int Minimax(int depth, Grid state, int currentPlayer,
            int alpha, int beta, ref int outBestMove, bool setBestMove)
        {
            Debug.Assert(depth >= 0);

            totalNodesSearched++;

            // Check if this state has already been visited.
            TTableEntry entry;
            if (transpositionTable.TryGet(state, out entry)
                && entry.Depth >= depth)
            {
                tableLookups++;

                switch (entry.NodeType)
                {
                    case NodeType.Exact:
                        if (setBestMove)
                        {
                            outBestMove = entry.BestMove;
                        }
                        return entry.Score;
                    case NodeType.Upper:
                        beta = Math.Min(beta, entry.Score);
                        break;
                    case NodeType.Lower:
                        alpha = Math.Max(alpha, entry.Score);
                        break;
                }
            }

            int gameOverResult = state.IsGameOver(1 - currentPlayer);

            // Return the maximum value if the player won the game.
            if (gameOverResult == player)
            {
                Debug.Assert(!setBestMove);

                endNodesSearched++;
                return infinity;
            }

            // Return the minimum value if the opposing player won the game.
            if (gameOverResult == 1 - player)
            {
                Debug.Assert(!setBestMove);

                endNodesSearched++;
                return -infinity;
            }

            // Evaluate the state if this is a terminal state.
            if (depth == 0)
            {
                Debug.Assert(!setBestMove);

                return state.GetPlayerStreaks(player);
            }

            // TODO: Use the transposition table entry even if the result
            // is not deep enough.

            // Otherwise, find the best move.
            bool maximise = currentPlayer == player;
            int bestMove = -1;
            int score = (maximise ? int.MinValue : int.MaxValue);

            for (int move = 0; move < state.Width; move++)
            {
                if (state.IsValidMove(move))
                {
                    state.Move(move, currentPlayer);
                    int childScore = Minimax(depth - 1, state, 1 - currentPlayer, alpha,
                        beta, ref bestMove, false);
                    state.UndoMove(move, currentPlayer);

                    if (maximise && childScore > score || !maximise && childScore < score)
                    {
                        score = childScore;
                        bestMove = move;

                        // Update alpha and beta values.
                        if (maximise)
                        {
                            alpha = score;
                        }
                        else
                        {
                            beta = score;
                        }

                        // If alpha or beta cutoff.
                        if (beta <= alpha)
                        {
                            NodeType type = (maximise ? NodeType.Lower : NodeType.Upper);

                            transpositionTable.Add(new TTableEntry(depth, bestMove,
                                state.GetTTableHash(), score, type));

                            if (setBestMove)
                            {
                                outBestMove = bestMove;
                            }

                            return score;
                        }
                    }
                }
            }

            Debug.Assert(bestMove != -1);

            transpositionTable.Add(new TTableEntry(depth, bestMove, state.GetTTableHash(),
                score, NodeType.Exact));

            if (setBestMove)
            {
                outBestMove = bestMove;
            }

            return score;
        }

        private void PrintMoveStatistics(double runtime, int score)
        {
            // Print the number of nodes looked at and the search time.
            double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
            Console.WriteLine("Done");
            Console.WriteLine("Analysed {0:N0} states, including {1:N0} end states.",
                totalNodesSearched, endNodesSearched);
            Console.WriteLine("Runtime {0:N} ms ({1:N} states / ms).", runtime,
                nodesPerMillisecond);

            if (score == infinity)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("AI win is guaranteed.");
                Console.ResetColor();
            }

            if (score == -infinity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("AI loss is guaranteed (Assuming perfect play).");
                Console.ResetColor();
            }

            // Print transposition table statistics.
            double standardDeviation;
            double averageBucketSize;
            double averageFullBucketSize;
            int fullBuckets;
            transpositionTable.TestUsage(out standardDeviation, out averageBucketSize,
                out averageFullBucketSize, out fullBuckets);

            Console.WriteLine("Transposition table:");
            Console.WriteLine("\tLookups:                  {0:N0}", tableLookups);
            Console.WriteLine("\tRequests:                 {0:N0}",
                transpositionTable.Requests);
            Console.WriteLine("\tCollisions:               {0:N0}",
                transpositionTable.Collisions);
            Console.WriteLine("\tItems:                    {0:N0}",
                transpositionTable.Size);
            Console.WriteLine("\tStandard deviation:       {0:N4}", standardDeviation);
            Console.WriteLine("\tAverage bucket size:      {0:N4}", averageBucketSize);
            Console.WriteLine("\tAverage full bucket size: {0:N4}", averageFullBucketSize);
            Console.WriteLine("\tFull buckets:             {0:N0}", fullBuckets);
            transpositionTable.ResetStatistics();

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
