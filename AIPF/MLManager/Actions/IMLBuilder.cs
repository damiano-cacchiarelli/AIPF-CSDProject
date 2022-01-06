using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public interface IMLBuilder
    {
        public MLContext MLContext { get; }
        public IMLBuilder Next { get; set; }

        void Fit(IDataView rawData, out IDataView transformedDataView);

        public object Predict(object toPredict);

        public List<MetricContainer> EvaluateAll(IDataView dataView);
    }
}
