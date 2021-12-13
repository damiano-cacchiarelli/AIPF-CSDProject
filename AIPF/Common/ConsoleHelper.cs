using System;

namespace AIPF.Common
{
    public class ConsoleHelper
    {
        public static int MaxCursorTop = 0;
        public static int NextCursorTop => MaxCursorTop+1;
        public static int CurrentCursorTop => Console.CursorTop;

        public static void Write(string msg)
        {
            Console.Write(msg);
            MaxCursorTop = Math.Max(MaxCursorTop, CurrentCursorTop);
        }

        public static void WriteLine(string msg)
        {
            var f = CurrentCursorTop;
            Console.WriteLine(msg);
            MaxCursorTop = Math.Max(MaxCursorTop, CurrentCursorTop);
        }
    }
}
