using Microsoft.ML;

namespace AIPF.MLManager.Metrics
{
    public interface IEvaluable
    {
        MetricContainer Evaluate(MLContext mlContext, IDataView data);
    }
}