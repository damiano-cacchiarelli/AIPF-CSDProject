using Microsoft.ML.Data;

namespace AIPF.MLManager.Metrics
{
    public static class EvaluateMetricsUtils
    {
        public static void AddInContainer(MetricContainer metricContainer, BinaryClassificationMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.Accuracy), metrics.Accuracy) { IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.PositiveRecall), metrics.PositiveRecall));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.NegativeRecall), metrics.NegativeRecall));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.PositivePrecision), metrics.PositivePrecision));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.NegativePrecision), metrics.NegativePrecision));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.F1Score), metrics.F1Score) { Min = 0, Max = 1, IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.AreaUnderRocCurve), metrics.AreaUnderRocCurve) { IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.AreaUnderPrecisionRecallCurve), metrics.AreaUnderPrecisionRecallCurve) { IsBetterIfCloserTo = 1 });
        }

        public static void AddInContainer(MetricContainer metricContainer, MulticlassClassificationMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MicroAccuracy), metrics.MicroAccuracy) { IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MacroAccuracy), metrics.MacroAccuracy) { IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LogLoss), metrics.LogLoss) { Min = 0, Max = 1, IsBetterIfCloserTo = 0 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LogLossReduction), metrics.LogLossReduction) { Min = double.NegativeInfinity, Max = 1, IsBetterIfCloserTo = 1 });
            for (int i = 0; i < metrics.PerClassLogLoss.Count; i++)
            {
                metricContainer.AddMetric(new MetricOptions(nameof(metrics.PerClassLogLoss) + i.ToString(), metrics.PerClassLogLoss[i]) { Min = 0, Max = 1, IsBetterIfCloserTo = 0 });
            }
        }

        public static void AddInContainer(MetricContainer metricContainer, RegressionMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LossFunction), metrics.LossFunction) { IsBetterIfCloserTo = 1 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MeanAbsoluteError), metrics.MeanAbsoluteError) { IsBetterIfCloserTo = 0 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MeanSquaredError), metrics.MeanSquaredError) { Min = 0, IsBetterIfCloserTo = 0 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.RootMeanSquaredError), metrics.RootMeanSquaredError) { IsBetterIfCloserTo = 0 });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.RSquared), metrics.RSquared) { Min = double.NegativeInfinity, Max = 1, IsBetterIfCloserTo = 1 });
        }
    }
}
