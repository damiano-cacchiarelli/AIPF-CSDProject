using Microsoft.ML;
using System;

namespace AIPF.MLManager.Modifiers
{
    public class ProgressIndicator<I> : IModifier<I, I>, ITotalNumberRequirement where I : class, ICopy<I>, new()
    {
        protected readonly string processName;
        public int Processed { get; protected set; }
        public int TotalCount { get; set; } = 1;

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
            Log();
        }

        protected virtual void Log()
        {
            if (Processed == 0) Console.WriteLine("");
            Console.WriteLine($"Progress Indicator work! {processName} - {Processed++}");
        }
    }
}
