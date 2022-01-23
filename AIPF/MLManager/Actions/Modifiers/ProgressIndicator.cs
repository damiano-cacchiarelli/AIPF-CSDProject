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

        protected IMessageQueue<double> messageQueue;

        public ProgressIndicator(string processName, IMessageQueue<double> messageQueue)
        {
            this.processName = processName;
            this.messageQueue = messageQueue;
            this.messageQueue.Register(processName);
        }

        public ProgressIndicator(string processName)
        {
            this.processName = processName;
        }

        void IModificator.Begin()
        {
            processed = 0;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, I>(MappingOperation, null);
        }

        private void MappingOperation(I input, I output)
        {
            input.Copy(ref output);
            lock (_sync)
            {
                if (processed+1 < TotalCount)
                {
                    Interlocked.Increment(ref processed);
                }
                messageQueue.EnqueueAsync(processName, ((double)processed) / TotalCount, CancellationToken.None);
            }
        }

        void IModificator.End()
        {
            messageQueue.EnqueueAsync(processName, 1, CancellationToken.None);
        }
    }
}
