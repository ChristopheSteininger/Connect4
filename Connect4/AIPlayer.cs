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
        private const int NodeTypeExact = 1;
        private const int NodeTypeUpper = 2;
        private const int NodeTypeLower = 3;
        private const int maxMoves = 7 * 6; // TODO: Parameterise this?

        // Search options.
        private readonly int moveLookAhead = 42;

        // Move ordering tables.
        private TranspositionTable transpositionTable = new TranspositionTable();
        private int[] staticMoveOrdering = new int[] { 3, 2, 4, 1, 5, 0, 6 };

        // Fields used during search.
        private int moveNumber = 6;
        private int finalMove;

        private AILog log;

        // Statistics for log.
        private bool printToConsole = true;
        private long totalNodesSearched;
        private long endNodesSearched;
        private long pvNodes;
        private long cutNodes;
        private long allNodes;
        private long shallowTableLookups;
        private long tableLookups;
        private long alphaBetaCutoffs;
        private long betaCutoffs;
        private long betaCutoffsOnFirstChild;
        private long betaCutoffsOnOrderedChildren;
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
            pvNodes = 0;
            cutNodes = 0;
            allNodes = 0;
            shallowTableLookups = 0;
            tableLookups = 0;
            alphaBetaCutoffs = 0;
            betaCutoffs = 0;
            betaCutoffsOnFirstChild = 0;
            betaCutoffsOnOrderedChildren = 0;

            // Print the start time of the search.
            string updateText = String.Format(
                "(Look ahead is {0}, began at {1})", moveLookAhead, DateTime.Now.TimeOfDay);
            Console.Write(updateText);

            DateTime startTime = DateTime.Now;
            
            int score = Negamax(moveNumber, grid, -infinity, infinity);

            double runtime = (DateTime.Now - startTime).TotalMilliseconds;
            totalRuntime += runtime;

            // Clear the start time text.
            Console.SetCursorPosition(Console.CursorLeft - updateText.Length,
                Console.CursorTop);
            Console.Write(new String(' ', updateText.Length));
            Console.SetCursorPosition(Console.CursorLeft - updateText.Length,
                Console.CursorTop);

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

            PrintMoveStatistics(runtime, grid, score, scores, scoreTypes);

            moveNumber += 2;

            // Store the result so the main thread can make the move.
            e.Result = finalMove;
        }

        public override void GameOver(bool winner)
        {
            log.EndGame(winner);
        }

        private int Negamax(int currentDepth, Grid state, int alpha, int beta)
        {
            int currentPlayer = currentDepth & 1;
            int originalAlpha = alpha;

            totalNodesSearched++;
            
            ulong validMovesMask = state.GetValidMovesMask();
            ulong playerThreats = state.GetThreats(currentPlayer);

            // If the player can win on this move then return immediately,
            // no more evaluation is needed.
            ulong currentPlayerThreats = validMovesMask & playerThreats;
            if (currentPlayerThreats != 0)
            {
                endNodesSearched++;

                if (currentDepth == moveNumber)
                {
                    finalMove = GetFirstColumnOfThreatBoard(currentPlayerThreats);
                }

                return infinity - currentDepth;
            }

            ulong opponentThreats = state.GetThreats(1 - currentPlayer);

            // If the opponent could win in one move on this position, then the
            // current player must play that move instead.
            ulong currentOpponentThreats = validMovesMask & opponentThreats;
            if (currentOpponentThreats != 0)
            {
                int forcedMove = GetFirstColumnOfThreatBoard(currentOpponentThreats);

                if (currentDepth == moveNumber)
                {
                    finalMove = forcedMove;
                }

                // Remove the forced move from the threat board.
                currentOpponentThreats &= ~(0x3FUL << (forcedMove * (state.Height + 1)));

                // If there is another threat return the loss immediately, otherwise
                // play the forced move.
                if (currentOpponentThreats != 0)
                {
                    endNodesSearched++;
                    return -infinity + currentDepth + 1;
                }

                // Take the single forced move.
                state.Move(forcedMove, currentPlayer);
                int childScore = -Negamax(currentDepth + 1, state, -beta, -alpha);
                state.UndoMove(forcedMove, currentPlayer);

                return childScore;
            }

            // This must be a draw if there are no forced moves and only two
            // moves left in the game.
            if (currentDepth >= maxMoves - 1)
            {
                endNodesSearched++;
                return 0;
            }

            // Will be set to the best move of the lookup.
            int entryBestMove;

            // Check if this state has already been visited.
            ulong entry;
            if (transpositionTable.Lookup(state, out entry, out entryBestMove))
            {
                // If the state has been visited, then return immediately or
                // improve the alpha and beta values depending on the node type.
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
                if (Math.Sign(alpha) >= Math.Sign(beta))
                {
                    alphaBetaCutoffs++;

                    if (currentDepth == moveNumber)
                    {
                        finalMove = entryBestMove;
                    }

                    return entryScore;
                }
            }

            // A bitmap where a 1 means the corresponding move has already been
            // checked. Initialised with a 1 at all invalid moves.
            ulong checkedMoves = state.GetInvalidMovesMask();

            int score = int.MinValue;

            int bestMove = -1;
            int index = -1;
            bool isFirstChild = true;

            // Find the best move recursively.
            // checkMoves will be equal to bottomRow when there are no more valid
            // moves.
            while (checkedMoves != Grid.bottomRow)
            {
                int move = GetNextMove(ref index, ref checkedMoves, entryBestMove,
                    validMovesMask, playerThreats);
                if (isFirstChild)
                {
                    bestMove = move;
                }

                // Apply the move and recurse.
                state.Move(move, currentPlayer);
                int childScore = -Negamax(currentDepth + 1, state, -beta, -alpha);
                state.UndoMove(move, currentPlayer);

                if (childScore > score)
                {
                    score = childScore;
                    bestMove = move;

                    if (score > alpha)
                    {
                        alpha = score;
                        // Check if this a cutoff.
                        if (Math.Sign(alpha) >= Math.Sign(beta))
                        {
                            betaCutoffs++;
                            if (isFirstChild)
                            {
                                betaCutoffsOnFirstChild++;
                            }
                            if (index <= 0)
                            {
                                betaCutoffsOnOrderedChildren++;
                            }

                            break;
                        }
                    }
                }

                isFirstChild = false;
            }

            Debug.Assert(bestMove != -1);

            // Determine the type of node, so the score can be used correctly
            // after a lookup.
            int flag;
            if (score <= originalAlpha)
            {
                flag = NodeTypeUpper;
                allNodes++;
            }
            else if (score >= beta)
            {
                flag = NodeTypeLower;
                cutNodes++;
            }
            else
            {
                flag = NodeTypeExact;
                pvNodes++;
            }

            // Store the score in the t-table in case the same state is reached later.
            transpositionTable.Add(currentDepth, bestMove, state.Hash, score, flag);

            if (currentDepth == moveNumber)
            {
                finalMove = bestMove;
            }

            return score;
        }

        private int GetFirstColumnOfThreatBoard(ulong threatBoard)
        {
            return((threatBoard & 0x3F) != 0) ? 0
                : ((threatBoard & (0x3FUL << 7)) != 0) ? 1
                : ((threatBoard & (0x3FUL << 14)) != 0) ? 2
                : ((threatBoard & (0x3FUL << 21)) != 0) ? 3
                : ((threatBoard & (0x3FUL << 28)) != 0) ? 4
                : ((threatBoard & (0x3FUL << 35)) != 0) ? 5
                : ((threatBoard & (0x3FUL << 42)) != 0) ? 6
                : -1;
        }

        private int GetNextMove(ref int i, ref ulong checkedMoves,
            int entryBestMove,
            ulong validMovesMask, ulong playerThreats)
        {
            int move;
            ulong moveMask;
            bool skipRuinThreat = false;

            ulong threatsAbove = playerThreats & (validMovesMask << 1);
            do
            {
                if (i == -1)
                {
                    move = entryBestMove;
                }
                else
                {
                    move = staticMoveOrdering[i % 7];

                    // A player should not in general play below their own threats.
                    skipRuinThreat = i < 7
                        && ((threatsAbove & (0x3FUL << (7 * move))) != 0);
                }

                i++;
                moveMask = 1UL << (7 * move);

            } while (move == -1 || skipRuinThreat || (checkedMoves & moveMask) == moveMask);

            checkedMoves |= moveMask;

            return move;
        }

        private void PrintMoveStatistics(double runtime, Grid grid, int score,
            int[] scores, int[] scoreTypes)
        {
            Debug.Assert(scores.Length == scoreTypes.Length);

            log.WriteLine("Done");

            // Print the grid before the AI's move to the log file.
            log.WriteLineToLog();
            log.WriteLineToLog("Grid before AI move:");
            log.WriteLineToLog(grid.ToString());
            log.WriteLineToLog();

            // Print node statistics.
            double cutoffsOnFirstChild = betaCutoffsOnFirstChild * 100D / betaCutoffs;
            double cutoffsOnOrderedChildren = betaCutoffsOnOrderedChildren * 100D / betaCutoffs;
            log.Write("Analysed ");
            WriteWithColor("{0:N0}", ConsoleColor.White, totalNodesSearched);
            log.WriteLine(" states, including {0:N0} end states.", endNodesSearched);
            log.WriteLine("   Searched {0:N0} pv-nodes, {1:N0} cut-nodes and {2:N0} all-nodes.",
                pvNodes, cutNodes, allNodes);
            log.WriteLine("   {0:N0} cutoffs from lookups.", alphaBetaCutoffs);
            log.WriteLine("   {0:N0} beta cutoffs ({1:N2}% on first child, {2:N2}% on "
                + "ordered children).", betaCutoffs, cutoffsOnFirstChild,
                cutoffsOnOrderedChildren);

            // Print runtime statistics.
            double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
            string minutes = (runtime > 60000)
                ? String.Format(" ({0:N2} minutes)", runtime / 60000.0) : "";
            log.Write("Runtime {0:N} ms{1} (", runtime, minutes);
            WriteWithColor("{0:N}", ConsoleColor.White, nodesPerMillisecond);
            log.WriteLine(" states / ms).");
            log.WriteLine("   Total runtime {0:N} ms.", totalRuntime);

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
                            log.WriteToLog("+\u221E");
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("+Inf");
                            Console.ResetColor();
                        }

                        else if (scores[i] == -infinity)
                        {
                            log.WriteToLog("-\u221E");
                            Console.ForegroundColor = ConsoleColor.Red;
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
                WriteLineWithColor("   AI will win latest on move {0}.",
                    ConsoleColor.Green, infinity - score);
            }
            else if (score < 0)
            {
                WriteLineWithColor("   AI will lose on move {0} (assuming perfect play).",
                    ConsoleColor.Red, infinity + score);
            }
            else
            {
                log.WriteLine("   Move score is {0}.", score);
            }

            log.WriteLine("   Move is {0:N0}.", finalMove);

            // Print transposition table statistics.
            log.WriteLine();
            log.WriteLine("Transposition table:");
            log.WriteLine("   Size:                     {0:N0}", TranspositionTable.TableSize);
            log.WriteLine("   Memory Space:             {0:N0} MB",
                TranspositionTable.MemorySpaceBytes / 1024 / 1024);
            log.WriteLine("   Shallow Lookups:          {0:N0}", shallowTableLookups);
            log.WriteLine("   Lookups:                  {0:N0}", tableLookups);
            log.WriteLine("   Requests:                 {0:N0}",
                transpositionTable.Requests);
            log.WriteLine("   Insertions:               {0:N0}",
                transpositionTable.Insertions);
            log.WriteLine("   Collisions:               {0:N0}",
                transpositionTable.Collisions);
            log.WriteLine("   Items:                    {0:N0} ({1:N4}% full)",
                transpositionTable.Size, 100.0 * transpositionTable.Size
                / TranspositionTable.TableSize);
            transpositionTable.ResetStatistics();

            log.WriteLine();
            log.WriteLine();
        }

        private void WriteLineWithColor(string text, ConsoleColor color, params object[] args)
        {
            WriteWithColor(text, color, args);
            log.WriteLine();
        }

        private void WriteWithColor(string text, ConsoleColor color, params object[] args)
        {
            text = (args != null) ? String.Format(text, args) : text;

            Console.ForegroundColor = color;
            log.Write(text);
            Console.ResetColor();
        }
    }
}
