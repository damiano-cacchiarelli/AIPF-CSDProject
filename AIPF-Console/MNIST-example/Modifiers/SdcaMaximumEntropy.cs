using AIPF.MLManager;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using AIPF_Console.MNIST_example.Model;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIPF_Console.MNIST_example.Modifiers
{
    public class SdcaMaximumEntropy : IModifier<ProcessedImage, OutputImage>, IEvaluable, ITrainerIterable
    {

        public int NumberOfIterations { get; }

        private dynamic defaultMetrics = new
        {
            labelColumnName = "Label",
            scoreColumnName = "Score"
        };

        public SdcaMaximumEntropy(int numberOfIteration = 10)
        {
            NumberOfIterations = numberOfIteration;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: NumberOfIterations)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(nameof(OutputImage.Digit), "Label"));
        }

        public MetricContainer Evaluate(MLContext mlContext, IDataView data)
        {
            MetricContainer metricContainer = new MetricContainer(nameof(SdcaMaximumEntropy));

            var metrics = mlContext.MulticlassClassification.Evaluate(data, labelColumnName: defaultMetrics.labelColumnName, scoreColumnName: defaultMetrics.scoreColumnName);
            EvaluateMetricsUtils.AddInContainer(metricContainer, metrics);
            return metricContainer;
        }
    }
}
