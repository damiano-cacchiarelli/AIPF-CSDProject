using AIPF.MLManager.Modifiers;
using Microsoft.ML;

namespace AIPF.MLManager
{
    public interface IModifier<I, O> : IModificator where I : class, new() where O : class, new()
    {
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext);
    }
}
