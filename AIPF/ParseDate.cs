using AIPF.Data;
using AIPF.MLManager;
using Microsoft.ML;

namespace AIPF
{
    internal class ParseDate<T> : IModifier<TaxiFareRaw, object>
    {
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            throw new System.NotImplementedException();
        }
    }
}