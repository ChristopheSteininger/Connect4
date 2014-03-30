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

        private readonly int moveLookAhead = 14;

        private TranspositionTable transpositionTable
            = new TranspositionTable();

        private int move = 0;

        private AILog log;

        // Statistics for log.
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
            log = new AILog(player, moveLookAhead, printToConsole);
        }

        public AIPlayer(int player, int moveLookAhead)
            : base(player)
        {
            this.moveLookAhead = moveLookAhead;

            log = new AILog(player, moveLookAhead, printToConsole);
        }

        public override int GetMove(Grid grid)
        {
            totalNodesSearched = 0;
            endNodesSearched = 0;
            shallowTableLookups = 0;
            tableLookups = 0;
            alphaBetaCuttoffs = 0;
            log.Write("Calculating move {0:N0} . . . ", move);

            int bestMove = -1;
            int score = -1;

            // Only update the streak count for the AI player, no need
            // the evaluate after the opposing player's move.
            grid.UpdateLazyStreakCountForPlayer[player] = true;
            grid.UpdateLazyStreakCountForPlayer[1 - player] = false;

            // Get the best move and measure the runtime.
            DateTime startTime = DateTime.Now;
            for (int depth = 1; depth < moveLookAhead; depth++)
            {
                score = Minimax(move, move + depth, grid, player, -infinity,
                    infinity, ref bestMove, true);
                grid.ClearMoveHistory();
            }
            double runtime = (DateTime.Now - startTime).TotalMilliseconds;
            totalRuntime += runtime;

            PrintMoveStatistics(runtime, grid, bestMove, score);

            move++;

            return bestMove;
        }

        public override void GameOver(bool winner)
        {
            log.EndGame(winner);
        }

        private int Minimax(int currentDepth, int searchDepth, Grid state, int currentPlayer,
            int alpha, int beta, ref int outBestMove, bool setBestMove)
        {
            Debug.Assert(currentDepth <= searchDepth);

            totalNodesSearched++;

            // Check if the previous player won on the last move.
            if (state.LazyIsGameOver(1 - currentPlayer))
            {
                Debug.Assert(!setBestMove);
                endNodesSearched++;

                // Return the maximum value if the player won the game
                // on the last move.
                if (currentPlayer != player)
                {
                    return infinity;
                }

                // Return the minimum value if the opposing player won the game.
                return -infinity;
            }

            // Evaluate the state if this is a terminal state.
            if (currentDepth == searchDepth)
            {
                Debug.Assert(!setBestMove);

                return state.StreakCount[player];
            }

            int[][] validMoves = state.GetValidMoves();

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
                if (entry.Depth >= searchDepth)
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

                    // Swap the first move and the best move from the entry.
                    int bestMoveIndex = validMoves[1][entry.BestMove];
                    int temp = validMoves[0][0];
                    validMoves[0][0] = entry.BestMove;
                    validMoves[0][bestMoveIndex] = temp;
                    validMoves[1][0] = bestMoveIndex;
                    validMoves[1][bestMoveIndex] = 0;

                    Debug.Assert(validMoves[0][0] == entry.BestMove);
                }
            }

            // Otherwise, find the best move.
            bool maximise = currentPlayer == player;
            int bestMove = -1;
            int dummy = 0;
            int score = (maximise) ? int.MinValue : int.MaxValue;

            for (int i = 0; i < validMoves[0].Length; i++)
            {
                int move = validMoves[0][i];

                state.Move(move, currentPlayer);
                int childScore = Minimax(currentDepth + 1, searchDepth, state, 1 - currentPlayer,
                    alpha, beta, ref dummy, false);
                state.UndoMove(move, currentPlayer);

                if ((maximise && childScore > score) || (!maximise && childScore < score))
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

                        NodeType type = (maximise) ? NodeType.Lower : NodeType.Upper;

                        transpositionTable.Add(new TTableEntry(searchDepth, bestMove,
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
            transpositionTable.Add(new TTableEntry(searchDepth, bestMove, state.GetTTableHash(),
                score, NodeType.Exact));

            if (setBestMove)
            {
                outBestMove = bestMove;
            }

            return score;
        }

        private void PrintMoveStatistics(double runtime, Grid grid, int move, int score)
        {
            log.WriteLine("Done");

            // Print the grid before the AI's move to the log file.
            log.WriteLineToLog();
            log.WriteLineToLog("Grid before AI move:");
            log.WriteLineToLog(grid.ToString());
            log.WriteLineToLog();

            // Print the number of nodes looked at and the search time.
            double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
            log.WriteLine("Analysed {0:N0} states, including {1:N0} end states.",
                totalNodesSearched, endNodesSearched);
            log.WriteLine("Runtime {0:N} ms ({1:N} states / ms).", runtime,
                nodesPerMillisecond);
            log.WriteLine("Total runtime {0:N} ms.", totalRuntime);
            log.WriteLine("{0:N0} alpha and beta cutoffs.", alphaBetaCuttoffs);

            log.WriteLine("Move is {0:N0}.", move);
            if (score == infinity)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                log.WriteLine("AI win is guaranteed.");
                Console.ResetColor();
            }

            else if (score == -infinity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                log.WriteLine("AI loss is guaranteed (Assuming perfect play).");
                Console.ResetColor();
            }

            else
            {
                log.WriteLine("Move score is {0:N0}.", score);
            }

            // Print transposition table statistics.
            double standardDeviation;
            double averageBucketSize;
            double averageFullBucketSize;
            int fullBuckets;
            transpositionTable.TestUsage(out standardDeviation, out averageBucketSize,
                out averageFullBucketSize, out fullBuckets);

            log.WriteLine();
            log.WriteLine("Transposition table:");
            log.WriteLine("\tShallow Lookups:          {0:N0}", shallowTableLookups);
            log.WriteLine("\tLookups:                  {0:N0}", tableLookups);
            log.WriteLine("\tRequests:                 {0:N0}",
                transpositionTable.Requests);
            log.WriteLine("\tInsertions:               {0:N0}",
                transpositionTable.Insertions);
            log.WriteLine("\tCollisions:               {0:N0}",
                transpositionTable.Collisions);
            log.WriteLine("\tItems:                    {0:N0}",
                transpositionTable.Size);
            log.WriteLine("\tStandard deviation:       {0:N4}", standardDeviation);
            log.WriteLine("\tAverage bucket size:      {0:N4}", averageBucketSize);
            log.WriteLine("\tAverage full bucket size: {0:N4}", averageFullBucketSize);
            log.WriteLine("\tFull buckets:             {0:N0}", fullBuckets);
            transpositionTable.ResetStatistics();

            log.WriteLine();
            log.WriteLine();
        }
    }
}
