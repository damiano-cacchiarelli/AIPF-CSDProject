using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public interface IModificator
    {
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext);
    }
}
