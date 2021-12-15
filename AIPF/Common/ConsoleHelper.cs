using System;

namespace AIPF.Common
{
    public class ConsoleHelper
    {
        public static int MaxCursorTop = 0;
        public static int NextCursorTop = 0;
        public static int CurrentCursorTop => Console.CursorTop;

        public static void Write(string msg)
        {
            Console.Write(msg);
            MaxCursorTop = Math.Max(MaxCursorTop, CurrentCursorTop);
            NextCursorTop = CurrentCursorTop == MaxCursorTop ? MaxCursorTop + 1 : NextCursorTop;
            Console.CursorTop = NextCursorTop;
        }

        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
            MaxCursorTop = Math.Max(MaxCursorTop, CurrentCursorTop);
            NextCursorTop = MaxCursorTop;
            Console.CursorTop = NextCursorTop;
            Console.CursorLeft = 0;
        }
    }
}
