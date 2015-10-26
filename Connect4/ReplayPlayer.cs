using System;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Connect4
{
    class ReplayPlayer : Player
    {
        private class LogReader
        {
            private readonly int width;
            private readonly int height;
            private readonly string gridTopBottom;

            private readonly StreamReader reader;
            private readonly TileState playerTile;

            private TileState[,] lastState;

            public LogReader(string log, int player, int width, int height)
            {
                Debug.Assert(File.Exists(log));

                this.width = width;
                this.height = height;
                this.gridTopBottom = "+" + new string('-', width) + "+";

                reader = new StreamReader(log);
                playerTile = (player == 0) ? TileState.Player1 : TileState.Player2;

                // Read in the empty board.
                lastState = GetNextState();
            }

            public int GetNextMove()
            {
                TileState[,] currentState = GetNextState();

                // Return the difference.
                for (int row = 0; row < height; row++)
                {
                    for (int column = 0; column < width; column++)
                    {
                        if (lastState[row, column] == TileState.Empty
                            && currentState[row, column] == playerTile)
                        {
                            lastState = currentState;
                            return column;
                        }
                    }
                }

                Debug.Fail("Found no difference between last and current board state.");

                return -1;
            }

            private TileState[,] GetNextState()
            {
                // Find the start of the grid.
                for (string line = reader.ReadLine(); !reader.EndOfStream && line != gridTopBottom;
                    line = reader.ReadLine()) ;

                if (reader.EndOfStream)
                {
                    // Switch to human player?
                }

                TileState[,] state = new TileState[height, width];

                // Read the grid.
                for (int row = height - 1; row >= 0; row--)
                {
                    string gridLine = reader.ReadLine();

                    Debug.Assert(gridLine != gridTopBottom, "Grid is too short.");
                    Debug.Assert(gridLine.StartsWith("|") && gridLine.EndsWith("|"),
                        "Grid line \"" + gridLine + "\" must start and end with \"|\"");
                    Debug.Assert(gridLine.Length == width + 2,
                        "Grid line \"" + gridLine + "\" should be " + (width + 2)
                        + " characters long");

                    for (int column = 0; column < width; column++)
                    {
                        state[row, column] = ToTileState(gridLine[column + 1]);
                    }
                }

                Debug.Assert(reader.ReadLine() == gridTopBottom, "Grid is too long.");

                return state;
            }

            private TileState ToTileState(char c)
            {
                TileState result = TileState.Empty;

                switch (c)
                {
                    case '\u00B7':
                        result = TileState.Empty;
                        break;

                    case '\u25CB':
                        result = TileState.Player1;
                        break;

                    case '\u25CF':
                        result = TileState.Player2;
                        break;
                    default:
                        Debug.Fail("Unexpected character '" + c + "'.");
                        break;
                }

                return result;
            }
        }

        private readonly string log;

        private readonly LogReader logReader;

        public ReplayPlayer(int currentPlayer, Board board, string log)
            : base(currentPlayer, board)
        {
            this.log = log;

            logReader = new LogReader(log, currentPlayer, board.Width, board.Height);;
        }

        public override void BeginMove(Grid grid)
        {
            board.Move(logReader.GetNextMove(), player);
        }
    }
}
