using AIPF.Images;
using AIPF.MLManager.Metrics;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;

namespace AIPF.MLManager.Modifiers
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

            MulticlassClassificationMetrics metric = mlContext.MulticlassClassification.Evaluate(data, labelColumnName: defaultMetrics.labelColumnName, scoreColumnName: defaultMetrics.scoreColumnName);
            metricContainer.AddMetric(new MetricOptions(nameof(metric.MicroAccuracy), metric.MicroAccuracy.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metric.MacroAccuracy), metric.MacroAccuracy.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metric.LogLoss), metric.LogLoss.ToString()) { Min = "0", Max = "1", IsBetterIfCloserTo = "0" });
            for (int i = 0; i < metric.PerClassLogLoss.Count; i++)
            {
                metricContainer.AddMetric(new MetricOptions(nameof(metric.PerClassLogLoss) + i.ToString(), metric.PerClassLogLoss[i].ToString()) { Min = "0", Max = "1", IsBetterIfCloserTo = "0" });
            }
            return metricContainer;
        }
    }
}
