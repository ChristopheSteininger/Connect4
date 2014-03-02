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
        private int shallowTableLookups;
        private int tableLookups;
        private int alphaBetaCuttoffs;
        private double totalRuntime = 0;

        public AIPlayer(int player)
            : base(player)
        {
            if (printToConsole)
            {
                StartConsole();
            }
        }

        public AIPlayer(int player, int moveLookAhead)
            : base(player)
        {
            this.moveLookAhead = moveLookAhead;

            if (printToConsole)
            {
                StartConsole();
            }
        }

        public override int GetMove(Grid grid)
        {
            if (printToConsole)
            {
                totalNodesSearched = 0;
                endNodesSearched = 0;
                shallowTableLookups = 0;
                tableLookups = 0;
                alphaBetaCuttoffs = 0;
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
            totalRuntime += runtime;
            
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

            bool gameOverResult = state.IsGameOver(1 - currentPlayer);

            // FIX THIS: The return values are different for the lazy is game over.
            if (gameOverResult)
            {
                Debug.Assert(!setBestMove);
                endNodesSearched++;

                // Return the maximum value if the player won the game.
                if (currentPlayer != player)
                {
                    return infinity;
                }

                // Return the minimum value if the opposing player won the game.
                return -infinity;
            }

            // Evaluate the state if this is a terminal state.
            if (depth == 0)
            {
                Debug.Assert(!setBestMove);

                return state.GetPlayerStreaks(player);
            }

            int[] validMoves = state.GetValidMoves();

            // If there are no valid moves, then this is a draw.
            if (validMoves.Length == 0)
            {
                Debug.Assert(!setBestMove);
                endNodesSearched++;

                return 0;
            }

            // Check if this state has already been visited.
            TTableEntry entry;
            if (transpositionTable.TryGet(state, out entry))
            {
                if (entry.Depth >= depth)
                {
                    tableLookups++;

                    switch (entry.NodeType)
                    {
                        // If the score is exact, there is no need to check anything
                        // else, so return the score of the entry.
                        case NodeType.Exact:
                            if (setBestMove)
                            {
                                outBestMove = entry.BestMove;
                            }
                            return entry.Score;

                        // If the entry score is an upper bound on the actual score,
                        // see if the current upper bound can be reduced.
                        case NodeType.Upper:
                            beta = Math.Min(beta, entry.Score);
                            break;

                        // If the entry score is a lower bound on the actual score,
                        // see if the current lower bound can be increased.
                        case NodeType.Lower:
                            alpha = Math.Max(alpha, entry.Score);
                            break;
                    }

                    // At this point alpha or beta may have been improved, so check if
                    // this is a cuttoff.
                    if (beta <= alpha)
                    {
                        // TODO: This assertion fails when the AI is guaranteed to win.
                        //Debug.Assert(!setBestMove);
                        alphaBetaCuttoffs++;

                        // TODO: Check that the lower bound should be returned here.
                        return alpha;
                    }
                }

                else
                {
                    shallowTableLookups++;

                    // Find the index of entry.BestMove in the validMoves array.
                    for (int i = 0; i < validMoves.Length; i++)
                    {
                        if (validMoves[i] == entry.BestMove)
                        {
                            int temp = validMoves[0];
                            validMoves[0] = entry.BestMove;
                            validMoves[i] = temp;

                            break;
                        }
                    }

                    Debug.Assert(validMoves[0] == entry.BestMove);
                }
            }

            // TODO: Use the transposition table entry even if the result
            // is not deep enough.

            // Otherwise, find the best move.
            bool maximise = currentPlayer == player;
            int bestMove = -1;
            int score = (maximise ? int.MinValue : int.MaxValue);

            for (int i = 0; i < validMoves.Length; i++)
            {
                int move = validMoves[i];

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
                        // TODO: This assertion fails when the AI is guaranteed to win.
                        //Debug.Assert(!setBestMove);
                        alphaBetaCuttoffs++;

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

            Debug.Assert(bestMove != -1);

            // Store the score in the t-table as an exact score in case the same state is
            // reached later.
            transpositionTable.Add(new TTableEntry(depth, bestMove, state.GetTTableHash(),
                score, NodeType.Exact));

            if (setBestMove)
            {
                outBestMove = bestMove;
            }

            return score;
        }

        private void StartConsole()
        {
            Console.Title = "Debugging output for AI player";
            Console.SetWindowSize(84, 60);

            Console.WriteLine("Player {0} with {1} move look ahead.", player, moveLookAhead);
            Console.WriteLine();
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
            Console.WriteLine("Total runtime {0:N} ms.", totalRuntime);
            Console.WriteLine("{0:N0} alpha and beta cutoffs.", alphaBetaCuttoffs);

            if (score == infinity)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("AI win is guaranteed.");
                Console.ResetColor();
            }

            else if (score == -infinity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("AI loss is guaranteed (Assuming perfect play).");
                Console.ResetColor();
            }

            else
            {
                Console.WriteLine("Move score is {0:N0}.", score);
            }

            // Print transposition table statistics.
            double standardDeviation;
            double averageBucketSize;
            double averageFullBucketSize;
            int fullBuckets;
            transpositionTable.TestUsage(out standardDeviation, out averageBucketSize,
                out averageFullBucketSize, out fullBuckets);

            Console.WriteLine("Transposition table:");
            Console.WriteLine("\tShallow Lookups:          {0:N0}", shallowTableLookups);
            Console.WriteLine("\tLookups:                  {0:N0}", tableLookups);
            Console.WriteLine("\tRequests:                 {0:N0}",
                transpositionTable.Requests);
            Console.WriteLine("\tInsertions:               {0:N0}",
                transpositionTable.Insertions);
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
