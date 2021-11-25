using System;

namespace AIPF.MLManager.Modifiers
{
    public class ProgressPercentageIndicator<I> : ProgressIndicator<I> where I : class, ICopy<I>, new()
    {
        public int TotalCount { get; set; }

        public ProgressPercentageIndicator(string processName, bool toString = false) : base(processName, toString)
        {
        }

        protected override void Log()
        {
            if (Processed == 0) Console.WriteLine("");
            Console.WriteLine($"Progress Indicator work! {processName} - { (Processed++ / TotalCount * 100) } %");
        }
    }
}
