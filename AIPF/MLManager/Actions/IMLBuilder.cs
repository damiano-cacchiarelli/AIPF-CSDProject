using Microsoft.ML;

namespace AIPF.MLManager.Actions
{
    public interface IMLBuilder
    {
        public MLContext MLContext { get; }
        public IMLBuilder Next { get; set; }

        void Fit(IDataView rawData, out IDataView transformedDataView);

        public object Predict(object toPredict);
    }
}
