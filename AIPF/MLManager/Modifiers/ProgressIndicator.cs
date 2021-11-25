using Microsoft.ML;
using System;

namespace AIPF.MLManager.Modifiers
{
    public class ProgressIndicator<I> : IModifier<I, I> where I : class, ICopy<I>, new()
    {
        protected readonly string processName;
        protected readonly bool toString;
        public int Processed { get; protected set; }

        public ProgressIndicator(string processName, bool toString = false)
        {
            this.processName = processName;
            this.toString = toString;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, I>(MappingOperation, null);
        }

        private void MappingOperation(I input, I output)
        {
            input.Copy(ref output);
            if (toString)
            {
                Console.WriteLine($"{input} - {output}");
            }
            Log();
        }

        protected virtual void Log()
        {
            if (Processed == 0) Console.WriteLine("");
            Console.WriteLine($"Progress Indicator work! {processName} - {Processed++}");
        }
    }
}
