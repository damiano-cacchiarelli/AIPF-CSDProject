using System;

namespace AIPF.MLManager.Modifiers
{
    public class ProgressPercentageIndicator<I> : ProgressIndicator<I> where I : class, ICopy<I>, new()
    {
        public float Percentage
        {
            get
            {
                return Math.Clamp(Processed++ * 100f / TotalCount, 0, 100);
            }
        }

        public ProgressPercentageIndicator(string processName) : base(processName)
        {
        }

        protected override void Log()
        {
            if (Processed == 0) Console.WriteLine("");
            Console.WriteLine($"{processName} - Work  in progress... { Percentage }%");
        }
    }
}
