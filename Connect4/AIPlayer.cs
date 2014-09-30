using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;

namespace Connect4
{
    class AIPlayer : Player
    {
        // Search constants.
        private const int infinity = 127;
        private const int killerMovesEntrySize = 2;
        private const int NodeTypeExact = 1;
        private const int NodeTypeUpper = 2;
        private const int NodeTypeLower = 3;
        private const int maxMoves = 7 * 6; // TODO: Parameterise this?

        // Search options.
        private readonly int moveLookAhead = 24;

        // Move ordering tables.
        private TranspositionTable transpositionTable = new TranspositionTable();
        private int[][] killerMovesTable;
        private int[] staticMoveOrdering = new int[] { 3, 2, 4, 1, 5, 0, 6 };

        // Fields used during search.
        private int moveNumber = 0;
        private int finalMove;

        private AILog log;

        // Statistics for log.
        private bool printToConsole = true;
        private long totalNodesSearched;
        private long endNodesSearched;
        private long shallowTableLookups;
        private long tableLookups;
        private long alphaBetaCutoffs;
        private long betaCutoffs;
        private double totalRuntime = 0;

        public AIPlayer(int player, Board board)
            : base(player, board)
        {
            CreateAIPlayer(board.Seed);
        }

        public AIPlayer(int player, int moveLookAhead, Board board)
            : base(player, board)
        {
            this.moveLookAhead = moveLookAhead;

            CreateAIPlayer(board.Seed);
        }

        private void CreateAIPlayer(int seed)
        {
            killerMovesTable = new int[maxMoves][];
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

        public override void BeginMove(Grid grid)
        {
            // Run CalculateNextMove in a worker thread, then call MakeMove in the
            // main thread.
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(CalculateNextMove);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MakeMove);

            worker.RunWorkerAsync(grid);
        }

        void MakeMove(object sender, RunWorkerCompletedEventArgs e)
        {
            int move = (int)e.Result;

            board.Move(move, player);
        }

        private void CalculateNextMove(object sender, DoWorkEventArgs e)
        {
            Grid grid = (Grid)e.Argument;

            Debug.Assert(grid.Width == 7);
            Debug.Assert(grid.Height == 6);

            log.Write("Calculating move {0} . . . ", moveNumber);

            // Reset statistics counters.
            totalNodesSearched = 0;
            endNodesSearched = 0;
            shallowTableLookups = 0;
            tableLookups = 0;
            alphaBetaCutoffs = 0;
            betaCutoffs = 0;

            // Calculate the depth of this search.
            int maxDepth = Math.Min(moveLookAhead, maxMoves - moveNumber);

            // The runtime of each iteration in milliseconds.
            double[] runtimes = new double[maxDepth];

            int score = -1;

            // Get the best move and measure the runtime.
            for (int depth = 1; depth <= maxDepth; depth++)
            {
                // The branching factor is too small after move 23 to continue using
                // iterative deepening.
                if (moveNumber + depth >= 23)
                {
                    depth = maxDepth;
                }

                // Print the start time of this iteration.
                string updateText = String.Format(
                    "(Current look ahead is {0}, began at {1})", depth, DateTime.Now.TimeOfDay);
                Console.Write(updateText);

                DateTime startTime = DateTime.Now;

                score = NegaScout(moveNumber, moveNumber + depth, grid, player,
                    -infinity, infinity);
                grid.ClearMoveHistory();

                runtimes[depth - 1] = (DateTime.Now - startTime).TotalMilliseconds;
                totalRuntime += runtimes[depth - 1];

                // Clear the iteration start time text.
                Console.SetCursorPosition(Console.CursorLeft - updateText.Length,
                    Console.CursorTop);
                Console.Write(new String(' ', updateText.Length));
                Console.SetCursorPosition(Console.CursorLeft - updateText.Length,
                    Console.CursorTop);
            }

            // Retrieve the score of each move.
            int[] scores = new int[grid.Width];
            int[] scoreTypes = new int[grid.Width];
            ulong entry;
            int dummy;
            for (int move = 0; move < grid.Width; move++)
            {
                if (grid.IsValidMove(move))
                {
                    grid.Move(move, player);

                    if (transpositionTable.Lookup(grid, out entry, out dummy))
                    {
                        scores[move] = -TranspositionTable.GetScore(entry);
                        scoreTypes[move] = TranspositionTable.GetNodeType(entry);
                    }
                    else
                    {
                        scoreTypes[move] = -1;
                    }

                    grid.UndoMove(move, player);
                }
            }

            PrintMoveStatistics(runtimes, grid, finalMove, score, scores, scoreTypes);

            moveNumber += 2;

            // Store the result so the main thread can make the move.
            e.Result = finalMove;
        }

        public override void GameOver(bool winner)
        {
            log.EndGame(winner);
        }

        private int NegaScout(int currentDepth, int searchDepth, Grid state, int currentPlayer,
            int alpha, int beta)
        {
            Debug.Assert(currentDepth <= searchDepth);

            totalNodesSearched++;

            // Check if the previous player won on the last move.
            if (state.LazyIsGameOver(1 - currentPlayer))
            {
                Debug.Assert(currentDepth != moveNumber);
                endNodesSearched++;

                // Return the score of losing on the last move.
                return -44 + currentDepth;
            }

            // If there are no valid moves, then this is a draw.
            if (currentDepth >= maxMoves)
            {
                Debug.Assert(currentDepth != moveNumber);
                endNodesSearched++;

                return 0;
            }

            // Evaluate the state if this is a terminal state.
            if (currentDepth == searchDepth)
            {
                Debug.Assert(currentDepth != moveNumber);

                return 0;
            }

            // Will be set to the best move of the lookup.
            int entryBestMove;

            // Check if this state has already been visited.
            ulong entry;
            if (transpositionTable.Lookup(state, out entry, out entryBestMove))
            {
                // The depth is stored in bits 0 to 5 of the entry.
                int entryDepth = (int)(entry & 0x3F);

                // If the state has been visited and searched at least as deep
                // as needed, then return immediately or improve the alpha and
                // beta values depending on the node type.
                if (entryDepth >= searchDepth)
                {
                    tableLookups++;

                    // The score is stored in bits 8 to 15 of the entry.
                    int entryScore = (int)(((entry >> 8) & 0xFF) - 128);

                    // The type is stored in bits 6 to 7 of the entry.
                    int entryType = (int)((entry >> 6) & 0x3);

                    switch (entryType)
                    {
                        // If the score is exact, there is no need to check anything
                        // else, so return the score of the entry.
                        case NodeTypeExact:
                            if (currentDepth == moveNumber)
                            {
                                finalMove = entryBestMove;
                            }
                            return entryScore;

                        // If the entry score is an upper bound on the actual score,
                        // see if the current upper bound can be reduced.
                        case NodeTypeUpper:
                            beta = Math.Min(beta, entryScore);
                            break;

                        // If the entry score is a lower bound on the actual score,
                        // see if the current lower bound can be increased.
                        case NodeTypeLower:
                            alpha = Math.Max(alpha, entryScore);
                            break;
                    }

                    // At this point alpha or beta may have been improved, so check if
                    // this is a cuttoff.
                    if (beta <= alpha)
                    {
                        alphaBetaCutoffs++;

                        if (currentDepth == moveNumber)
                        {
                            finalMove = entryBestMove;
                        }

                        return entryScore;
                    }
                }

                // If this state has been visited but not searched deep enough,
                // use the best move the lookup as a clue to which move might be
                // best.
                else
                {
                    shallowTableLookups++;
                }
            }

            int childScore;

            // Check for forced moves.
            for (int move = 0; move < state.Width; move++)
            {
                if (state.IsValidMove(move))
                {
                    // TODO: Run this check in a seperate loop first?

                    // If the player can win on this move then return immediately,
                    // no more evaluation is needed.
                    if (state.LazyIsGameOverOnMove(currentPlayer, move))
                    {
                        if (currentDepth == moveNumber)
                        {
                            finalMove = move;
                        }

                        return 43 - currentDepth;
                    }

                    // If the opponent can win with this move, then the current player
                    // must play that move instead.
                    if (state.LazyIsGameOverOnMove(1 - currentPlayer, move))
                    {
                        state.Move(move, currentPlayer);
                        childScore = -NegaScout(currentDepth + 1, searchDepth, state,
                            1 - currentPlayer, -beta, -alpha);
                        state.UndoMove(move, currentPlayer);

                        if (currentDepth == moveNumber)
                        {
                            finalMove = move;
                        }

                        return childScore;
                    }
                }
            }

            int bestMove = -1;

            int betaScout = beta;

            int[] killerMoves = killerMovesTable[currentDepth];
            int orderedMoves = 1 + killerMovesEntrySize;

            // A bitmap where a 1 means the corresponding move has already been
            // checked. Initialised with a 1 at all invalid moves.
            uint checkedMoves = state.GetInvalidMovesMask();

            // checkMoves will be equal to this when there are no more valid
            // moves.
            uint allMovesChecked = ((uint)1 << state.Width) - 1;

            bool isFirstChild = true;

            // Assume that this is an all-node and therefore the score is an
            // upper bound on the true score until proven otherwise.
            int flag = NodeTypeUpper;

            int movesSearched = 0;

            // Find the best move recursively. A negative i means
            // use the shallow lookup or killer move.
            for (int i = -orderedMoves; i < state.Width
                && checkedMoves != allMovesChecked; i++)
            {
                int move;

                // Use shallow moves first.
                if (i == -orderedMoves)
                {
                    move = entryBestMove;
                }

                // Use the killer moves next.
                else if (i < 0)
                {
                    move = killerMoves[i + killerMovesEntrySize];
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
                // as invalid for future iterations.
                checkedMoves |= moveMask;

                if (isFirstChild)
                {
                    bestMove = move;
                }

                // Apply the move and recurse.
                state.Move(move, currentPlayer);

                childScore = -NegaScout(currentDepth + 1, searchDepth, state,
                    1 - currentPlayer, -betaScout, -alpha);

                // Rerun the search with a wider window if the returned score
                // is inside the bounds and this is a PV-node.
                if (alpha < childScore && childScore < beta && !isFirstChild)
                {
                    state.SetLastMove(move);
                    alpha = -NegaScout(currentDepth + 1, searchDepth, state,
                        1 - currentPlayer, -beta, -childScore);

                    bestMove = move;
                    flag = NodeTypeExact;
                }

                state.UndoMove(move, currentPlayer);

                // Check if this is a PV-node and the lower bound can be improved.
                if (childScore > alpha)
                {
                    alpha = childScore;
                    flag = NodeTypeExact;
                    bestMove = move;
                }

                // Check if this a beta cutoff. AKA a fail-high.
                if (alpha >= beta)
                {
                    betaCutoffs++;

                    // Remember this move as a killer move at the current depth.
                    killerMoves[1] = killerMoves[0];
                    killerMoves[0] = bestMove;

                    // This is a cut-node, so the score is a lower bound.
                    flag = NodeTypeLower;

                    break;
                }

                // Update the null window.
                betaScout = alpha + 1;

                isFirstChild = false;
                movesSearched++;
            }

            Debug.Assert(bestMove != -1);

            // Store the score in the t-table in case the same state is reached later.
            transpositionTable.Add(searchDepth, bestMove, state.Hash, alpha, flag);

            if (currentDepth == moveNumber)
            {
                finalMove = bestMove;
            }

            return alpha;
        }

        private void PrintMoveStatistics(double[] runtimes, Grid grid, int move,
            int score, int[] scores, int[] scoreTypes)
        {
            Debug.Assert(scores.Length == scoreTypes.Length);

            log.WriteLine("Done");

            // Calculate the total runtime.
            double runtime = 0;
            for (int i = 0; i < runtimes.Length; i++)
            {
                runtime += runtimes[i];
            }

            // Print the grid before the AI's move to the log file.
            log.WriteLineToLog();
            log.WriteLineToLog("Grid before AI move:");
            log.WriteLineToLog(grid.ToString());
            log.WriteLineToLog();

            // Print the number of nodes looked at and the search time.
            double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
            string minutes = (runtime > 60000)
                ? String.Format(" ({0:N2} minutes)", runtime / 60000.0) : "";
            log.WriteLine("Analysed {0:N0} states, including {1:N0} end states.",
                totalNodesSearched, endNodesSearched);
            log.WriteLine("Runtime {0:N} ms{1} ({2:N} states / ms).", runtime,
                minutes, nodesPerMillisecond);
            log.WriteLine("Total runtime {0:N} ms.", totalRuntime);
            log.WriteLine("{0:N0} cutoffs from lookups and {1:N0} beta cutoffs.",
                alphaBetaCutoffs, betaCutoffs);

            // Print the scores of each valid move.
            log.Write("The move scores are");
            for (int i = 0; i < scores.Length; i++)
            {
                if (grid.IsValidMove(i))
                {
                    log.Write("  {0}:", i);

                    if (scoreTypes[i] == -1)
                    {
                        log.Write("-");
                    }

                    else
                    {
                        if (scores[i] == infinity)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            log.WriteToLog("+\u221E");
                            Console.Write("+Inf");
                            Console.ResetColor();
                        }

                        else if (scores[i] == -infinity)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            log.WriteToLog("-\u221E");
                            Console.Write("-Inf");
                            Console.ResetColor();
                        }

                        else
                        {
                            log.Write("{0}", scores[i]);
                        }

                        switch (scoreTypes[i])
                        {
                            case NodeTypeExact:
                                log.Write("E");
                                break;
                            case NodeTypeLower:
                                log.Write("L");
                                break;
                            case NodeTypeUpper:
                                log.Write("U");
                                break;
                        }
                    }
                }
            }

            log.WriteLine();

            // Print the score of the chosen move.
            if (score > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                log.WriteLine("AI will win latest on move {0}.", 43 - score);
                Console.ResetColor();
            }
            else if (score < 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                log.WriteLine("AI will lose on move {0} (assuming perfect play).",
                    43 + score);
                Console.ResetColor();
            }

            log.WriteLine("Move is {0:N0}.", move);

            // Print the runtime of each iteration.
            log.WriteLine();
            log.WriteLine("Iteration runtimes:");

            for (int i = 0; i < runtimes.Length; i++)
            {
                log.Write(" +{0,-2}: {1,8:N2} ms", i + 1, runtimes[i]);

                // Print the percentage of time spent in this iteration.
                log.Write(", {0,15:N2}%", runtimes[i] * 100 / runtime);

                // Print how much longer this iteration took than the last.
                if (i > 0)
                {
                    double increase = runtimes[i] / runtimes[i - 1];
                    if (!Double.IsInfinity(increase) && !Double.IsNaN(increase)
                        && increase > 0.1)
                    {
                        log.Write(" (\u00D7 {0:N2})", increase);
                    }
                }

                log.WriteLine();
            }

            // Print transposition table statistics.
            log.WriteLine();
            log.WriteLine("Transposition table:");
            log.WriteLine("\tSize:                     {0:N0}", TranspositionTable.TableSize);
            log.WriteLine("\tMemory Space:             {0:N0} MB",
                TranspositionTable.MemorySpaceBytes / 1024 / 1024);
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
