using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public class ConcatenateColumn<I> : IModifier<I, I> where I : class, new()
    {
        private readonly string input;
        private readonly string output;

        public ConcatenateColumn(string input, string output)
        {
            this.input = input;
            this.output = output;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.Concatenate(output, input);
        }
    }
}
