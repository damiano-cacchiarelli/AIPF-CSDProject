using System;

namespace AIPF.Common
{
    public class ConsoleProgress : IProgress<double>
    {
        static private readonly object _sync = new object();

        private string progressName;
        private string complete;
        private int currentCursorTop;

        public ConsoleProgress(string progressName, string complete = "Complete!")
        {
            this.progressName = progressName;
            this.complete = complete;
            currentCursorTop = ConsoleHelper.NextCursorTop;
        }

        public void Report(double progress)
        {
            int percentage = Math.Clamp((int)(100 * progress), 1, 100);
            lock (_sync)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = currentCursorTop;
                //string toWrite = $"{progressName} [{new string('=', percentage / 2)}] {(percentage >= 100 ? complete : percentage.ToString()  +  '%')}";
                ConsoleHelper.Write($"{progressName} [");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.CursorTop = currentCursorTop;
                ConsoleHelper.Write($"{ new string('=', percentage / 2)}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorTop = currentCursorTop;
                ConsoleHelper.Write($"] { (percentage >= 100 ? complete : percentage.ToString() + '%')}");
                Console.CursorTop = ConsoleHelper.NextCursorTop;
                Console.CursorLeft = 0;
            }
        }
    }
}
