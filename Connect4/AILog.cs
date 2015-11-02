using System;
using System.IO;

namespace Connect4
{
    class AILog
    {
        // Log folders.
        private const String logRootFolder = "..\\Logs\\";
        private const String allFolder     = logRootFolder + "All Logs\\";
        private const String winFolder     = logRootFolder + "Win Log Copies\\";
        private const String lossFolder    = logRootFolder + "Loss Log Copies\\";

        // Special log files.
        private const String mostRecentGameLog   = logRootFolder + "Log 0 - Most Recent Game.txt";
        private const String mostRecentAIWinLog  = logRootFolder + "Log 0 - Most Recent AI Win.txt";
        private const String mostRecentAILossLog = logRootFolder + "Log 0 - Most Recent AI Loss.txt";

        private String logName;
        private String logPathAndName;

        private bool printToConsole;

        public AILog(int player, int seed, int moveLookAhead, bool printToConsole)
        {
            this.printToConsole = printToConsole;

            DateTime now = DateTime.Now;

            // Set the name of this logfile.
            string timestampFormat = "yyyy-M-d H-m-s";
            logName = "Log " + now.ToString(timestampFormat) + " la="
                + moveLookAhead + ".txt";
            logPathAndName = allFolder + logName;

            // Create the log folders if they do not exist already.
            CreateFolders();

            if (printToConsole)
            {
                Console.Title = "Debugging output for AI player";
                Console.SetWindowSize(84, 60);
            }

            string fileDateTimeFormatA = "h:mm tt, d";
            string fileDateTimeFormatB = "MMMM yyyy";
            WriteLine("Connect 4 AI Log - Created at {0} of {1}.",
                now.ToString(fileDateTimeFormatA), now.ToString(fileDateTimeFormatB));
                
#if DEBUG
            WriteLine("Debug build.");
#else
            WriteLine("Release build.");
#endif
            WriteLine("Player {0} with {1} move look ahead.", player, moveLookAhead);
            WriteLine("Seed is {0}", seed);
            Write(Get32BitWarning());

            WriteLine();
        }

        private string Get32BitWarning()
        {
            return (Environment.Is64BitProcess)
                ? ""
                : "Warning! Running in 32 bit mode. Search will take longer." + Environment.NewLine;
        }

        private void CreateFolders()
        {
            EnsureFolderExists(logRootFolder);
            EnsureFolderExists(allFolder);
            EnsureFolderExists(winFolder);
            EnsureFolderExists(lossFolder);
        }

        // Make sure that the given folder exists.
        private void EnsureFolderExists(String folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

        // Copy the log to the win or loss folder depending on the outcome of the
        // game. Also update the most recent shortcut log files.
        public void EndGame(bool AIWon)
        {
            WriteLine("The AI " + (AIWon ? "wins" : "loses") + "!");

            CopyLogTo(mostRecentGameLog);

            if (AIWon)
            {
                CopyLogTo(winFolder + logName);
                CopyLogTo(mostRecentAIWinLog);
            }

            else
            {
                CopyLogTo(lossFolder + logName);
                CopyLogTo(mostRecentAILossLog);
            }
        }

        private void CopyLogTo(String copy)
        {
            File.Copy(logPathAndName, copy, true);
        }

        // Wrapper functions to write to the log file and/or console:

        public void WriteLineToLog(String message = "", params object[] args)
        {
            OutputMessage(message, true, false, true, args);
        }

        public void WriteToLog(String message, params object[] args)
        {
            OutputMessage(message, true, false, false, args);
        }

        public void WriteLine(String message = "", params object[] args)
        {
            OutputMessage(message, true, true, true, args);
        }

        public void Write(String message, params object[] args)
        {
            OutputMessage(message, true, true, false, args);
        }

        private void OutputMessage(String message, bool writeToLog, bool writeToConsole,
            bool newline, params object[] args)
        {
            message = (args != null) ? String.Format(message, args) : message;
            message += (newline) ? Environment.NewLine : "";

            if (writeToLog)
            {
                using (StreamWriter writer = new StreamWriter(logPathAndName, true))
                {
                    writer.Write(message);
                }
            }

            if (writeToConsole && printToConsole)
            {
                Console.Write(message);
            }
        }
    }
}
