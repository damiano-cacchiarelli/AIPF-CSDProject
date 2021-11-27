using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using System;
using System.Collections.Generic;

namespace AIPF.MLManager
{
    public class MLManager<I, O> where I : class, new() where O : class, new()
    {
        private readonly MLContext mlContext;

        private List<IPipeline> pipelines = new List<IPipeline>();
        private IEstimator<ITransformer> pipeline;
        private ITransformer model;
        private PredictionEngine<I, O> predictionEngine;

        public MLManager()
        {
            mlContext = new MLContext();
        }

        public Pipeline<R> CreatePipeline<R>(IModifier<I, R> modifier) where R : class, new()
        {
            // TODO: remove modifier.GetPipeline(mlContext)
            return new Pipeline<R>(mlContext, UpdatePipeline, modifier, modifier.GetPipeline(mlContext));
        }

        private void UpdatePipeline(IPipeline pipeline)
        {
            pipelines.Add(pipeline);
            this.pipeline = pipeline.GetPipeline();
        }

        // TODO: remove
        private void UpdatePipeline(IEstimator<ITransformer> pipeline)
        {
            this.pipeline = pipeline;
        }

        public void Fit(IEnumerable<I> rawImages, out IDataView transformedDataView)
        {
            if (pipeline == null)
                throw new Exception("The pipeline must be valid");
            
            foreach  (var p in pipelines)
            {
                ITotalNumberRequirement it = p.GetModificator() as ITotalNumberRequirement;
                if(p.GetModificator() is ITotalNumberRequirement)
                {
                    (p.GetModificator() as ITotalNumberRequirement).TotalCount = new List<I>(rawImages).Count;
                }
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

        public IEnumerable<O> GetEnumerable(IDataView transformedDataView)
        {
            return mlContext.Data.CreateEnumerable<O>(transformedDataView,
                reuseRowObject: true);
        }
    }
}
