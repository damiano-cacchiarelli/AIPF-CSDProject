using AIPF.MLManager.Metrics;
using Microsoft.ML;

namespace AIPF.MLManager.Modifiers.TaxiFare
{
    public class ApplyOnnxModel<I, O> : IModifier<I, O>, IEvaluable
    {
        private string modelPath;
        private string[] inputColumnNames;
        private string[] outputColumnNames;

        private string algo;

        public ApplyOnnxModel(string modelPath, string[] inputColumnNames = null, string[] outputColumnNames = null, string algo = null)
        {
            this.modelPath = modelPath;
            this.inputColumnNames = inputColumnNames;
            this.outputColumnNames = outputColumnNames;
            this.algo = algo;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            inputColumnNames ??= new string[] { };
            outputColumnNames ??= new string[] { };
            return mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, outputColumnNames: outputColumnNames, inputColumnNames: inputColumnNames);
        }

        public MetricContainer Evaluate(MLContext mlContext, IDataView data)
        {
            var metricContainer = new MetricContainer(algo);
            if (algo != null && algo.Equals("Binary"))
            {
                var metrics = mlContext.BinaryClassification.Evaluate(data);
                metricContainer.AddMetric(new MetricOptions(nameof(metrics.F1Score), metrics.F1Score.ToString()));
            }
            return metricContainer;
        }
    }
}
