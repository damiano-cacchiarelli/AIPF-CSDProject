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

        private IPipeline linkedPipeline;
        
        //private IEstimator<ITransformer> pipeline;
        private ITransformer model;
        private PredictionEngine<I, O> predictionEngine;

        public MLManager()
        {
            mlContext = new MLContext();
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

        public void Fit(IEnumerable<I> rawImages, out IDataView transformedDataView)
        {
            var pipeline = linkedPipeline.GetPipeline(mlContext);

            if (pipeline == null)
                throw new Exception("The pipeline must be valid");
            
            // Injecting some values inside the modifiers
            foreach  (var totalNumberRequirement in GetTransformersOfPipeline<ITotalNumberRequirement>())
            {
                totalNumberRequirement.TotalCount = new List<I>(rawImages).Count;
            }
            
            IDataView data = mlContext.Data.LoadFromEnumerable(rawImages);
            model = pipeline.Fit(data);
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

        public void EvaluateAll(IDataView processedData)
        {
            List<MetricContainer> metrics = new List<MetricContainer>();
            foreach (var evaluable in GetTransformersOfPipeline<IEvaluable>())
            {
                metrics.Add(evaluable.Evaluate(mlContext, processedData));
            }
            Console.WriteLine("\n========= Metrics =========");
            metrics.ForEach(Console.WriteLine);
            if(metrics.Count == 0) Console.WriteLine("No metrics available");
            Console.WriteLine("========= ------- =========\n");
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

        public IEnumerable<O> GetEnumerable(IDataView transformedDataView)
        {
            return mlContext.Data.CreateEnumerable<O>(transformedDataView,
                reuseRowObject: true);
        }
    }
}
