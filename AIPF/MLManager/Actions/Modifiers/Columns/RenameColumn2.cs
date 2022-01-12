using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public class RenameColumn2<I> : IModifier<I, I> where I : class, new()
    {
        private readonly string input;
        private readonly string output;

        public RenameColumn2(string input, string output)
        {
            this.input = input;
            this.output = output;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CopyColumns(output, input);
        }
    }
}
