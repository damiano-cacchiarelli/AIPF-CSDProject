using AIPF.MLManager.Modifiers;
using Microsoft.ML;

namespace AIPF.MLManager
{
    public interface IPipeline
    {
        IModificator GetModificator();

        IEstimator<ITransformer> GetPipeline();
    }
}
