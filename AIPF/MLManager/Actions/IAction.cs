using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public interface IAction
    {
        void Execute(IDataView dataView, out IDataView trasformedDataView);

        List<MetricContainer> Evaluate(IDataView dataView, out IDataView transformedDataView);
    }
}
