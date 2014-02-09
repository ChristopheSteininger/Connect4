using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Connect4
{
    static class Logger
    {
        private static StreamWriter writer;
        private static int level = 0;

        public static void StartLog(string logFilename)
        {
            writer = new StreamWriter(logFilename);
        }

        public static void EnterNewLevel()
        {
            level++;
        }

        public static void ExitLevel()
        {
            level--;
        }

        public static void Log(string message)
        {
            //return;
            for (int i = 0; i < level; i++)
            {
                writer.Write("\t");
            }
            writer.WriteLine(message);
        }

        public static void Close()
        {
            writer.Close();
        }
    }
}
