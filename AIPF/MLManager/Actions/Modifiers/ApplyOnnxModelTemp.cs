using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIPF.MLManager.Modifiers.TaxiFare
{
    public class ApplyOnnxModelTemp<I, O, E> : IModifier<I, O>, IEvaluable where O : class, new() where E : class, new()
    {
        private string modelPath;
        private string[] inputColumnNames;
        private string[] outputColumnNames;

        private string algo = "Multiclass";
        private Action<O, E> mapping;/* = (input, output) => {
            output.ScoreSingle = input.Score[0];
            output.PredictedLabelSingle = input.PredictedLabel[0];
            output.ProbabilitySingle = input.Probability[0];
        };*/

        public ApplyOnnxModelTemp(string modelPath, Action<O, E> mapping = null)
        {
            this.modelPath = modelPath;
            this.mapping = mapping;
        }

        public ApplyOnnxModelTemp(string modelPath, string[] inputColumnNames = null, string[] outputColumnNames = null, Action<O, E> mapping = null)
        {
            this.modelPath = modelPath;
            this.inputColumnNames = inputColumnNames;
            this.outputColumnNames = outputColumnNames;
            this.mapping = mapping;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            inputColumnNames ??= new string[] { };
            outputColumnNames ??= new string[] { };
            return mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, outputColumnNames: outputColumnNames, inputColumnNames: inputColumnNames);
        }

        public MetricContainer Evaluate(MLContext mlContext, IDataView data)
        {
            if (algo == null) return null;

            var columns = EvaluateColumn.GetDictionary(typeof(E));

            if (algo.Equals("Binary"))
            {
                var metricContainer = new MetricContainer(algo);
                var normData = mlContext.Transforms.CustomMapping(mapping, null).Fit(data).Transform(data);

                var metrics = mlContext.BinaryClassification.Evaluate(normData, 
                    labelColumnName: columns["labelColumnName"], 
                    scoreColumnName: columns["scoreColumnName"], 
                    probabilityColumnName: columns["probabilityColumnName"], 
                    predictedLabelColumnName: columns["predictedLabelColumnName"]);

                metricContainer.AddMetric(new MetricOptions(nameof(metrics.Accuracy), metrics.Accuracy.ToString()));
                return metricContainer;
            }
            else if (algo.Equals("Multiclass"))
            {
                var metricContainer = new MetricContainer(algo);
                var normData = mlContext.Transforms.CustomMapping(mapping, null).Fit(data).Transform(data);

                var metrics = mlContext.MulticlassClassification.Evaluate(normData,
                    labelColumnName: "EventType",//columns["labelColumnName"],
                    scoreColumnName: columns["scoreColumnName"],
                    predictedLabelColumnName: columns["predictedLabelColumnName"]);

                metricContainer.AddMetric(new MetricOptions(nameof(metrics.MacroAccuracy), metrics.MacroAccuracy.ToString()));
                return metricContainer;
            }
            return null;
        }
    }
}
