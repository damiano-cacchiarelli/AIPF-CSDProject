using AIPF.Images;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections.Generic;

namespace AIPF.MLManager.Modifiers
{
    public class SdcaMaximumEntropy : IModifier<ProcessedImage, OutputImage>, IEvaluable
    {
        private int numberOfIteration;

        private dynamic defaultMetrics = new
        {
            labelColumnName = "Label",
            scoreColumnName = "Score"
        };

        public SdcaMaximumEntropy(int numberOfIteration = 10)
        {
            this.numberOfIteration = numberOfIteration;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: numberOfIteration)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(nameof(OutputImage.Digit), "Label"));
        }

        public Metric Evaluate(MLContext mlContext, IDataView data)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            MulticlassClassificationMetrics metric = mlContext.MulticlassClassification.Evaluate(data, labelColumnName: defaultMetrics.labelColumnName, scoreColumnName: defaultMetrics.scoreColumnName);
            dictionary.Add(nameof(metric.MicroAccuracy), metric.MicroAccuracy.ToString());
            dictionary.Add(nameof(metric.MacroAccuracy), metric.MacroAccuracy.ToString());
            dictionary.Add(nameof(metric.LogLoss), metric.LogLoss.ToString());
            for (int i = 0; i < metric.PerClassLogLoss.Count; i++)
            {
                dictionary.Add(nameof(metric.PerClassLogLoss) + i.ToString(), metric.PerClassLogLoss[i].ToString());
            } 
            return new Metric(nameof(SdcaMaximumEntropy), dictionary);
        }
    }
}
