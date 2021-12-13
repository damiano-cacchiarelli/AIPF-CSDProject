using System;

namespace AIPF.Common
{
    public class ConsoleProgress : IProgress<double>
    {
        static private readonly object _sync = new object();

        private string progressName;
        private int currentCursorTop;

        public ConsoleProgress(string progressName)
        {
            this.progressName = progressName;
            currentCursorTop = ConsoleHelper.NextCursorTop;
        }

        public void Report(double progress)
        {
            int percentage = (int)(100 * progress);
            lock (_sync)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = currentCursorTop;
                ConsoleHelper.Write(progressName + " [" + new string('=', percentage / 2) + "] " + percentage + "%");
                Console.CursorTop = ConsoleHelper.NextCursorTop;
                Console.CursorLeft = 0;
            }
        }
    }
}
