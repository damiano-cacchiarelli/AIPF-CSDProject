using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using System.Collections.Generic;
using System.Linq;

namespace AIPF.MLManager.Actions
{
    public interface IPipeline : IEnumerable<IPipeline>
    {
        public IPipeline Next { get; }
        public IModificator Modificator { get; }

        List<IModificator> GetModificators();

        IEstimator<ITransformer> GetPipeline(MLContext mlContext);

        IEnumerable<T> GetTransformersOfPipeline<T>() where T : class
        {
            return GetModificators().Where(m => m is T).Select(m => m as T);
        }
    }
}
