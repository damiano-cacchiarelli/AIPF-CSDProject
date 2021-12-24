using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager
{
    public interface IMLBuilder : IEnumerable<IMLBuilder>
    {
        public MLContext MLContext { get; }
        public IMLBuilder Next { get; set; }

        void Fit(IDataView rawData, out IDataView transformedDataView);

        public object Predict(object toPredict);
    }
}
