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
        private const int killerMovesEntrySize = 2;

        // TODO: Parameterise this?
        private const int maxMoves = 7 * 6;

        private readonly int moveLookAhead = 15;

        private TranspositionTable transpositionTable
            = new TranspositionTable();

        private int[][] killerMovesTable;

        private int moveNumber = 0;

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
            CreateAIPlayer();
        }

        public AIPlayer(int player, int moveLookAhead)
            : base(player)
        {
            this.moveLookAhead = moveLookAhead;

            CreateAIPlayer();
        }

        private void CreateAIPlayer()
        {
            killerMovesTable = new int[maxMoves - 1][];
            for (int i = 0; i < killerMovesTable.Length; i++)
            {
                killerMovesTable[i] = new int[killerMovesEntrySize];
            }

            log = new AILog(player, moveLookAhead, printToConsole);
        }

        public override int GetMove(Grid grid)
        {
            totalNodesSearched = 0;
            endNodesSearched = 0;
            shallowTableLookups = 0;
            tableLookups = 0;
            alphaBetaCuttoffs = 0;
            log.Write("Calculating move {0:N0} . . . ", moveNumber);

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
                score = Minimax(moveNumber, moveNumber + depth, grid, player, -infinity,
                    infinity, ref bestMove, true);
                grid.ClearMoveHistory();
            }
            double runtime = (DateTime.Now - startTime).TotalMilliseconds;
            totalRuntime += runtime;

            PrintMoveStatistics(runtime, grid, bestMove, score);

            moveNumber++;

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

            // Will be set to the best move of a shallow lookup.
            int shallowLookup = -1;

            // Check if this state has already been visited.
            TTableEntry entry;
            if (transpositionTable.TryGet(state, out entry))
            {
                // If the state has been visited and searched at least as deep
                // as needed, then return immediately or improve the alpha and
                // beta values depending on the node type.
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

                // If this state has been visited but not searched deep enough,
                // use the best move the lookup as a clue to which move might be
                // best.
                else
                {
                    shallowTableLookups++;
                    shallowLookup = entry.BestMove;
                }
            }

            bool maximise = currentPlayer == player;
            int bestMove = -1;
            int dummy = 0;
            int score = (maximise) ? int.MinValue : int.MaxValue;

            int[] killerMoves = killerMovesTable[currentDepth];
            int orderedMoves = 1 + killerMovesEntrySize;

            // A bit vector where a 1 means the corresponding move has
            // already been checked. Initialised to the invalid moves.
            uint checkedMoves = state.GetInvalidMovesMask();

            // Find the best move recurrsively. A negative i means
            // use an ordered move.
            for (int i = -orderedMoves; i < state.Width; i++)
            {
                int move = i;

                // Use the killer moves first.
                if (i < -1)
                {
                    move = killerMoves[i + orderedMoves];
                }

                // Use shallow moves next.
                else if (i == -1)
                {
                    move = shallowLookup;
                }

                // Only proceed if the move is valid and has not been checked yet.
                uint moveMask = (uint)1 << move;
                if (move < 0 || (checkedMoves & moveMask) == moveMask)
                {
                    continue;
                }

                checkedMoves |= moveMask;

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

                        // Store the current score as an upper or lower bound on the exact
                        // score.
                        NodeType type = (maximise) ? NodeType.Lower : NodeType.Upper;
                        transpositionTable.Add(new TTableEntry(searchDepth, bestMove,
                            state.GetTTableHash(), score, type));

                        // Remember this move as a killer move at the current depth.
                        if (killerMoves[killerMovesEntrySize - 1] == 0)
                        {
                            killerMoves[killerMovesEntrySize - 1] = bestMove;
                        }
                        else
                        {
                            killerMoves[0] = bestMove;
                        }

                        if (setBestMove)
                        {
                            outBestMove = bestMove;
                        }

                        return score;
                    }
                }
            }

            // If there are no valid moves, then this is a draw.
            if (bestMove == -1)
            {
                Debug.Assert(!setBestMove);
                endNodesSearched++;

                return 0;
            }

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
            log.WriteLine();
            log.WriteLine("Transposition table:");
            log.WriteLine("\tSize:                     {0:N0}", TranspositionTable.TableSize);
            log.WriteLine("\tSearch Size:              {0:N0}", TranspositionTable.SearchSize);
            log.WriteLine("\tShallow Lookups:          {0:N0}", shallowTableLookups);
            log.WriteLine("\tLookups:                  {0:N0}", tableLookups);
            log.WriteLine("\tRequests:                 {0:N0}",
                transpositionTable.Requests);
            log.WriteLine("\tInsertions:               {0:N0}",
                transpositionTable.Insertions);
            log.WriteLine("\tCollisions:               {0:N0}",
                transpositionTable.Collisions);
            log.WriteLine("\tItems:                    {0:N0} ({1:N4}% full)",
                transpositionTable.Size, 100.0 * transpositionTable.Size
                / TranspositionTable.TableSize);
            transpositionTable.ResetStatistics();

            log.WriteLine();
            log.WriteLine();
        }
    }
}
