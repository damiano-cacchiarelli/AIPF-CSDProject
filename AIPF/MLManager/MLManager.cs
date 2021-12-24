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

        private MLBuilder<I, O> mlBuilder;

        //private IPipeline linkedPipeline;
        private IDataView testData = null;
        private IDataView trainData = null;

        //private ITransformer model;
        //private PredictionEngine<I, O> predictionEngine;

        public MLLoader<I> MlLoader { get; private set; }

        public MLManager()
        {
            mlContext = new MLContext();
            mlContext.Log += new EventHandler<LoggingEventArgs>(Log);
            MlLoader = new MLLoader<I>(this.mlContext);
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            if(e.Source.Contains("SdcaTrainerBase"))
                ConsoleHelper.WriteLine(sender.GetType() + " " + e.Message);
        }

       public MLBuilder<I, O> CreatePipeline()
        {
            mlBuilder = new MLBuilder<I, O>(mlContext);
            return mlBuilder;
        }

        public void Fit(IEnumerable<I> rawData, out IDataView transformedDataView)
        {
            Fit(mlContext.Data.LoadFromEnumerable(rawData), out transformedDataView);
        }

        public void Fit(IDataView rawData, out IDataView transformedDataView)
        {
            if(mlBuilder == null)
                throw new Exception("The pipeline must be valid");

            DataOperationsCatalog.TrainTestData dataSplit = mlContext.Data.TrainTestSplit(rawData, testFraction: 0.2);
            trainData = dataSplit.TrainSet;
            testData = dataSplit.TestSet;
            transformedDataView = rawData;

            foreach (var builder in mlBuilder)
            {
                builder.Fit(transformedDataView, out transformedDataView);
            }
        }

        public O Predict(I imageToPredict)
        {
            if (mlBuilder == null)
                throw new Exception("The pipeline must be valid");

            try
            {
                return mlBuilder.Predict(imageToPredict);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return default(O);
            }
        }

        /*



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
        */
    }
}
