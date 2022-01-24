using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public interface IModificator
    {
        public void Begin() { }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext);

        public void End() { }
    }
}
