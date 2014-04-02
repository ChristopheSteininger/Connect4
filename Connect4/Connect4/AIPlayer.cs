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

        private readonly int moveLookAhead = 16;

        private TranspositionTable transpositionTable
            = new TranspositionTable();

        private int[][] killerMovesTable;

        private int[] staticMoveOrdering = new int[] { 1, 5, 4, 3, 2, 0, 6 };

        private int moveNumber = 0;

        private AILog log;

        // Statistics for log.
        private bool printToConsole = true;
        private int totalNodesSearched;
        private int endNodesSearched;
        private int shallowTableLookups;
        private int tableLookups;
        private int alphaBetaCutoffs;
        private int alphaCutoffs;
        private int betaCutoffs;
        private double totalRuntime = 0;

        public AIPlayer(int player, int seed)
            : base(player)
        {
            CreateAIPlayer(seed);
        }

        public AIPlayer(int player, int moveLookAhead, int seed)
            : base(player)
        {
            this.moveLookAhead = moveLookAhead;

            CreateAIPlayer(seed);
        }

        private void CreateAIPlayer(int seed)
        {
            killerMovesTable = new int[maxMoves - 1][];
            for (int i = 0; i < killerMovesTable.Length; i++)
            {
                killerMovesTable[i] = new int[killerMovesEntrySize];
                for (int j = 0; j < killerMovesEntrySize; j++)
                {
                    killerMovesTable[i][j] = -1;
                }
            }

            log = new AILog(player, seed, moveLookAhead, printToConsole);
        }

        public override int GetMove(Grid grid)
        {
            Debug.Assert(grid.Width == 7);
            Debug.Assert(grid.Height == 6);

            log.Write("Calculating move {0} . . . ", moveNumber);

            // Reset statistics counters.
            totalNodesSearched = 0;
            endNodesSearched = 0;
            shallowTableLookups = 0;
            tableLookups = 0;
            alphaBetaCutoffs = 0;
            alphaCutoffs = 0;
            betaCutoffs = 0;

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

            moveNumber += 2;

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
                        alphaBetaCutoffs++;

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

            // A bitmap where a 1 means the corresponding move has already been
            // checked. Initialised with a 1 at all invalid moves.
            uint checkedMoves = state.GetInvalidMovesMask();

            // checkMoves will be equal to this when there are no more valid
            // moves.
            uint allMovesChecked = ((uint)1 << state.Width) - 1;

            // Find the best move recurrsively. A negative i means
            // use the shallow lookup or killer move.
            for (int i = -orderedMoves; i < state.Width
                && checkedMoves != allMovesChecked; i++)
            {
                int move;

                // Use shallow moves first.
                if (i == -orderedMoves)
                {
                    move = shallowLookup;
                }

                // Use the killer moves next.
                else if (i < 0)
                {
                    move = killerMoves[i + orderedMoves - 1];
                }

                // Otherwise, use the static move ordering.
                else
                {
                    move = staticMoveOrdering[i];
                }

                // Only proceed if the move is valid and has not been checked yet.
                uint moveMask = (uint)1 << move;
                if (move < 0 || (checkedMoves & moveMask) == moveMask)
                {
                    continue;
                }

                // At this point, the move has been found, so mark the move
                // as invalid.
                checkedMoves |= moveMask;

                // Apply the move and recurse.
                state.Move(move, currentPlayer);
                int childScore = Minimax(currentDepth + 1, searchDepth, state, 1 - currentPlayer,
                    alpha, beta, ref dummy, false);
                state.UndoMove(move, currentPlayer);

                // If it is the maximising player's turn and this is a new maximum,
                // update alpha and check for a beta cutoff.
                if (maximise && childScore > score)
                {
                    score = childScore;
                    alpha = childScore;
                    bestMove = move;

                    // beta cutoff
                    if (score >= beta)
                    {
                        alphaBetaCutoffs++;
                        betaCutoffs++;

                        // Store the current score as a lower bound on the exact score.
                        transpositionTable.Add(new TTableEntry(searchDepth, bestMove,
                            state.GetTTableHash(), score, NodeType.Lower));

                        // Remember this move as a killer move at the current depth.
                        killerMoves[1] = killerMoves[0];
                        killerMoves[0] = bestMove;

                        if (setBestMove)
                        {
                            outBestMove = bestMove;
                        }

                        return score;
                    }
                }

                // If this is the minimising player's turn and this is new minimum,
                // update beta and check for alpha cutoff.
                else if (!maximise && childScore < score)
                {
                    score = childScore;
                    beta = childScore;
                    bestMove = move;

                    // alpha cutoff.
                    if (score <= alpha)
                    {
                        alphaBetaCutoffs++;
                        alphaCutoffs++;

                        // Store the current score as an upper bound on the exact score.
                        transpositionTable.Add(new TTableEntry(searchDepth, bestMove,
                            state.GetTTableHash(), score, NodeType.Upper));

                        // Remember this move as a killer move at the current depth.
                        killerMoves[1] = killerMoves[0];
                        killerMoves[0] = bestMove;

                        Debug.Assert(!setBestMove);
                        return score;
                    }
                }
            }

            // If there are no valid moves, then this is a draw.
            if (bestMove == -1)
            {
                endNodesSearched++;

                Debug.Assert(!setBestMove);
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
            log.WriteLine("{0:N0} alpha and beta cutoffs.", alphaBetaCutoffs);
            log.WriteLine("\t(including {0:N0} alpha and {1:N0} beta cutoffs)",
                alphaCutoffs, betaCutoffs);

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
