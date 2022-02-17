using Microsoft.ML.Data;

namespace AIPF.MLManager.Metrics
{
    public static class EvaluateMetricsUtils
    {
        public static void AddInContainer(MetricContainer metricContainer, BinaryClassificationMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.Accuracy), metrics.Accuracy.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.PositiveRecall), metrics.PositiveRecall.ToString()));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.NegativeRecall), metrics.NegativeRecall.ToString()));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.PositivePrecision), metrics.PositivePrecision.ToString()));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.NegativePrecision), metrics.NegativePrecision.ToString()));
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.F1Score), metrics.F1Score.ToString()) { Min = "0", Max = "1", IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.AreaUnderRocCurve), metrics.AreaUnderRocCurve.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.AreaUnderPrecisionRecallCurve), metrics.AreaUnderPrecisionRecallCurve.ToString()) { IsBetterIfCloserTo = "1" });
        }

        public static void AddInContainer(MetricContainer metricContainer, MulticlassClassificationMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MicroAccuracy), metrics.MicroAccuracy.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MacroAccuracy), metrics.MacroAccuracy.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LogLoss), metrics.LogLoss.ToString()) { Min = "0", Max = "1", IsBetterIfCloserTo = "0" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LogLossReduction), metrics.LogLossReduction.ToString()) { Min = "-inf", Max = "1", IsBetterIfCloserTo = "1" });
            for (int i = 0; i < metrics.PerClassLogLoss.Count; i++)
            {
                metricContainer.AddMetric(new MetricOptions(nameof(metrics.PerClassLogLoss) + i.ToString(), metrics.PerClassLogLoss[i].ToString()) { Min = "0", Max = "1", IsBetterIfCloserTo = "0" });
            }
        }

        public static void AddInContainer(MetricContainer metricContainer, RegressionMetrics metrics)
        {
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.LossFunction), metrics.LossFunction.ToString()) { IsBetterIfCloserTo = "1" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MeanAbsoluteError), metrics.MeanAbsoluteError.ToString()) { IsBetterIfCloserTo = "0" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.MeanSquaredError), metrics.MeanSquaredError.ToString()) { Min = "0", IsBetterIfCloserTo = "0" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.RootMeanSquaredError), metrics.RootMeanSquaredError.ToString()) { IsBetterIfCloserTo = "0" });
            metricContainer.AddMetric(new MetricOptions(nameof(metrics.RSquared), metrics.RSquared.ToString()) { Min="-inf", Max="1", IsBetterIfCloserTo = "1" });
        }
    }
}
