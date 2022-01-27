using Microsoft.ML;

namespace AIPF.MLManager.Actions.Modifiers.Columns
{
    public class RenameColumn<I> : IModifier<I, I> where I : class, new()
    {
        private readonly string input;
        private readonly string output;

        public RenameColumn(string input, string output)
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
