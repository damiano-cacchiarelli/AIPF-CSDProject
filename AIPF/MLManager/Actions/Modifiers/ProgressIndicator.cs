using AIPF.MLManager.EventQueue;
using Microsoft.ML;
using System.Threading;

namespace AIPF.MLManager.Actions.Modifiers
{
    public class ProgressIndicator<I> : IModifier<I, I>, ITotalNumberRequirement where I : class, ICopy<I>, new()
    {

        static private readonly object _sync = new object();

        protected readonly string processName;
        private int processed = 0;
        public int Processed { get => processed; protected set => processed = value; }
        public int TotalCount { get; set; } = 1;

        public ProgressIndicator(string processName)
        {
            MessageManager.IMessageQueue.Register(processName);
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
                var count = ((double)processed) / TotalCount;
                MessageManager.IMessageQueue.EnqueueAsync(processName, count, CancellationToken.None);
            }
        }

        void IModificator.End()
        {
            MessageManager.IMessageQueue.EnqueueAsync(processName, 1, CancellationToken.None);
        }
    }
}
