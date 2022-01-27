using Microsoft.ML;

namespace AIPF.MLManager.Actions.Modifiers.Columns
{
    public class MapValueToKey<I> : IModifier<I, I> where I : class, new()
    {
        private readonly string input;
        private readonly string output;

        public MapValueToKey(string input, string output)
        {
            this.input = input;
            this.output = output;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.Conversion.MapValueToKey(output, input);
        }
    }
}
