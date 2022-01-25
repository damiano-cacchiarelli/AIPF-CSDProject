using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Linq;

namespace AIPF.MLManager.Modifiers.TaxiFare
{
    public class ApplyEvaluableOnnxModel<I, O, E> : ApplyOnnxModel<I, O>, IEvaluable where O : class, new() where E : class, new()
    {
        private EvaluateAlgorithmType? algorithmType;
        private EvaluateAlgorithm evaluateAlgorithmAttribute;
        private readonly Action<O, E> mapping;

        public ApplyEvaluableOnnxModel(string modelPath, Action<O, E> mapping, string[] inputColumnNames = null, string[] outputColumnNames = null)
             : base(modelPath, inputColumnNames, outputColumnNames)
        {
            this.mapping = mapping;
            DefineAlgorithmType();
        }

        public MetricContainer Evaluate(MLContext mlContext, IDataView data)
        {
            if (algorithmType == null) return null;

            var columns = EvaluateColumn.GetDictionary(typeof(E));
            var metricContainer = new MetricContainer($"ONNX - {algorithmType}");
            var normData = mlContext.Transforms.CustomMapping(mapping, null).Fit(data).Transform(data);

            switch (algorithmType)
            {
                case EvaluateAlgorithmType.BINARY:
                    var binaryMetrics = mlContext.BinaryClassification.Evaluate(normData,
                        labelColumnName: evaluateAlgorithmAttribute.GetDictionary()["labelColumnName"],
                        scoreColumnName: columns["scoreColumnName"],
                        probabilityColumnName: columns["probabilityColumnName"],
                        predictedLabelColumnName: columns["predictedLabelColumnName"]);
                    EvaluateMetricsUtils.AddInContainer(metricContainer, binaryMetrics);
                    break;
                case EvaluateAlgorithmType.MULTICLASS:
                    var multiclassMetrics = mlContext.MulticlassClassification.Evaluate(normData,
                        labelColumnName: evaluateAlgorithmAttribute.GetDictionary()["labelColumnName"],
                        scoreColumnName: columns["scoreColumnName"],
                        predictedLabelColumnName: columns["predictedLabelColumnName"]);
                    EvaluateMetricsUtils.AddInContainer(metricContainer, multiclassMetrics);
                    break;
                case EvaluateAlgorithmType.REGRESSION:
                    var regressionMetrics = mlContext.Regression.Evaluate(normData,
                        labelColumnName: evaluateAlgorithmAttribute.GetDictionary()["labelColumnName"],
                        scoreColumnName: columns["scoreColumnName"]);
                    EvaluateMetricsUtils.AddInContainer(metricContainer, regressionMetrics);
                    break;
                case EvaluateAlgorithmType.CLUSTERING:
                    throw new NotImplementedException();
                default:
                    break;
            }
            return metricContainer;
        }

        private void DefineAlgorithmType()
        {
            evaluateAlgorithmAttribute = Attribute.GetCustomAttributes(typeof(E), typeof(EvaluateAlgorithm))
                .Select(a => a as EvaluateAlgorithm)
                .FirstOrDefault();
            algorithmType = evaluateAlgorithmAttribute?.GetAlgorithmType();
        }
    }
}
