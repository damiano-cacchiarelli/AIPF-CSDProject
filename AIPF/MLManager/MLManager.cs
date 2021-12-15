using AIPF.Common;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIPF.MLManager
{
    public class MLManager<I, O> where I : class, new() where O : class, new()
    {
        private readonly MLContext mlContext;

        //Create a Class with this 3 fields
        private IPipeline linkedPipeline;
        private IDataView testData = null;
        private IDataView trainData = null;

        private ITransformer model;
        private PredictionEngine<I, O> predictionEngine;

        public MLManager()
        {
            mlContext = new MLContext();
            //mlContext.Log += new EventHandler<LoggingEventArgs>(Log);
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            if(e.Source.Contains("SdcaTrainerBase"))
                ConsoleHelper.WriteLine(sender.GetType() + " " + e.Message);
        }

        public Pipeline<R> CreatePipeline<R>(IModifier<I, R> modifier) where R : class, new()
        {
            var pipeline = new Pipeline<R>(modifier);
            linkedPipeline = pipeline;
            return pipeline;
        }

        public void PrintPipelineStructure()
        {
            linkedPipeline.PrintPipelineStructure();
        }

        public void Fit(IEnumerable<I> rawData, out IDataView transformedDataView)
        {
            var pipeline = linkedPipeline.GetPipeline(mlContext);

            if (pipeline == null)
                throw new Exception("The pipeline must be valid");
            
            // Injecting some values inside the modifiers
            var nI = 0;
            foreach (var trainerIterable in GetTransformersOfPipeline<ITrainerIterable>())
            {
                nI = Math.Max(nI,trainerIterable.NumberOfIterations);
            }
            foreach (var totalNumberRequirement in GetTransformersOfPipeline<ITotalNumberRequirement>())
            {
                totalNumberRequirement.TotalCount = (int)(new List<I>(rawData).Count * 0.8 * nI + 7778);
            }

            IDataView data = mlContext.Data.LoadFromEnumerable(rawData);

            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
            trainData = dataSplit.TrainSet;
            testData = dataSplit.TestSet;
           
            model = pipeline.Fit(trainData);
            transformedDataView = model.Transform(data);
        }

        public O Predict(I imageToPredict)
        {
            if (model == null)
                throw new Exception("You first need to define a model (Fit() must be called before)");
            if (predictionEngine == null)
                predictionEngine = mlContext.Model.CreatePredictionEngine<I, O>(model);

            return predictionEngine.Predict(imageToPredict);
        }

        public List<MetricContainer> EvaluateAll(IDataView dataView = null)
        {
            if (model == null)
                throw new Exception("You first need to define a model (Fit() must be called before)");
            if ((dataView ??= testData) == null)
                throw new Exception("Something went wrong during the Fit()!");
            
            var testDataView = model.Transform(dataView);
            List<MetricContainer> metrics = new List<MetricContainer>();
            foreach (var evaluable in GetTransformersOfPipeline<IEvaluable>())
            {
                metrics.Add(evaluable.Evaluate(mlContext, testDataView));
            }
            return metrics;
        }

        public IEnumerable<O> GetEnumerable(IDataView transformedDataView)
        {
            return mlContext.Data.CreateEnumerable<O>(transformedDataView,
                reuseRowObject: true);
        }

        private List<T> GetTransformersOfPipeline<T>() where T : class
        {
            List<T> transformers = new List<T>();
            foreach (var p in linkedPipeline)
            {
                if (p.GetModificator() is T)
                {
                    transformers.Add(p.GetModificator() as T);
                }
            }
            return transformers;
        }
    }
}
