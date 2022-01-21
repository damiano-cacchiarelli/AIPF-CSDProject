using AIPF.MLManager.EventQueue;
using Microsoft.ML;
using System;
using System.Threading;

namespace AIPF.MLManager.Modifiers
{
    public class ProgressIndicator<I> : IModifier<I, I>, ITotalNumberRequirement where I : class, ICopy<I>, new()
    {

        static private readonly object _sync = new object();

        protected readonly string processName;
        private int processed = 0;
        public int Processed { get => processed; protected set => processed = value; }
        public int TotalCount { get; set; } = 1;

        protected IMessageQueue<int> messageQueue;

        public ProgressIndicator(string processName, IMessageQueue<int> messageQueue)
        {
            this.processName = processName;
            this.messageQueue = messageQueue;
            this.messageQueue.Register(processName);
        }

        public ProgressIndicator(string processName)
        {
            this.processName = processName;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, I>(MappingOperation, null);
        }

        private void MappingOperation(I input, I output)
        {
            input.Copy(ref output);
            Interlocked.Increment(ref processed);
            messageQueue.EnqueueAsync(processName, processed, CancellationToken.None);
        }

        protected virtual void Log()
        {
            /*
            lock (_sync)
            {
                if (Processed == 0) ConsoleHelper.WriteLine("");
                Interlocked.Increment(ref processed);
                consoleProgress.Report((double)Processed / TotalCount);
                //ConsoleHelper.WriteLine($"{processName} - Work  in progress... {Processed} / {TotalCount}");
            }*/
        }
    }
}
