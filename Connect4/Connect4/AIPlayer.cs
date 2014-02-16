using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Connect4
{
    class AIPlayer : Player
    {
        private readonly int moveLookAhead = 10;
        private MinimaxNode root;

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

            // Get the best move and measure the runtime.
            DateTime startTime = DateTime.Now;
            root = Minimax(moveLookAhead, grid, player, -MinimaxNode.Infinity,
                MinimaxNode.Infinity);
            double runtime = (DateTime.Now - startTime).TotalMilliseconds;
            
            if (printToConsole)
            {
                PrintMoveStatistics(runtime);
            }

            return root.BestMove;
        }

        private void PrintMoveStatistics(double runtime)
        {
            // Print the number of nodes looked at and the search time.
            double nodesPerMillisecond = Math.Round(totalNodesSearched / runtime, 4);
            Console.WriteLine("Done");
            Console.WriteLine("Analysed {0:N0} states, including {1:N0} end states.",
                totalNodesSearched, endNodesSearched);
            Console.WriteLine("Runtime {0} ms ({1} states / ms).", runtime,
                nodesPerMillisecond);

            if (root.BestChild.Score == MinimaxNode.Infinity)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("AI win is guaranteed.");
                Console.ResetColor();
            }

            if (root.BestChild.Score == -MinimaxNode.Infinity)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("AI loss is guaranteed (Assuming perfect play).");
                Console.ResetColor();
            }

            // Print the scores of the child and grandchild states.
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

            Console.WriteLine("(Best opponent move is {0}).",
                root.BestChild.BestMove);

            Console.WriteLine();
            Console.WriteLine();
        }

        private MinimaxNode Minimax(int depth, Grid state, int currentPlayer,
            int alpha, int beta)
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
                        return new MinimaxNode(entry.Score);
                    case NodeType.Upper:
                        beta = Math.Min(beta, entry.Score);
                        break;
                    case NodeType.Lower:
                        alpha = Math.Max(alpha, entry.Score);
                        break;
                }
            }

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

            // TODO: Use the transposition table entry even if the result
            // is not deep enough.

            // Otherwise, find the best move.
            MinimaxNode result = new MinimaxNode(currentPlayer == player);
            for (int move = 0; move < state.Width; move++)
            {
                if (state.IsValidMove(move))
                {
                    state.Move(move, currentPlayer);
                    MinimaxNode child = Minimax(depth - 1, state, 1 - currentPlayer,
                        alpha, beta);
                    state.UndoMove(move, currentPlayer);
                    result.AddChild(child, move);

                    // Update alpha and beta values.
                    if (currentPlayer == player)
                    {
                        alpha = result.Score;

                        // Beta cutoff.
                        if (beta <= alpha)
                        {
                            transpositionTable.Add(new TTableEntry(
                                new Grid(state), depth, beta, NodeType.Lower));

                            return result;
                        }
                    }

                    else
                    {
                        beta = result.Score;

                        // Alpha cutoff.
                        if (beta <= alpha)
                        {
                            transpositionTable.Add(new TTableEntry(
                                new Grid(state), depth, alpha, NodeType.Upper));

                            return result;
                        }
                    }
                }
            }

            transpositionTable.Add(new TTableEntry(
                new Grid(state), depth, result.Score, NodeType.Exact));

            return result;
        }

        private int EvaluateState(Grid state)
        {
            return state.GetPlayerStreaks(player);
        }
    }
}
