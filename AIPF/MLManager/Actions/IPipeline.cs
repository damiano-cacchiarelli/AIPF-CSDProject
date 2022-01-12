using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public interface IPipeline : IEnumerable<IPipeline>
    {
        IPipeline GetNext();

        IModificator GetModificator();

        IEstimator<ITransformer> GetPipeline(MLContext mlContext);

        void PrintPipelineStructure();
    }
}
