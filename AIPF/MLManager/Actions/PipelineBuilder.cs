using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using System;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public class PipelineBuilder<I, O> : ITransformerAction<I, O> where I : class, new() where O : class, new()
    {
        private readonly MLContext mlContext;
        private IMLBuilder mlBuilder;
        private IPipeline linkedPipeline;

        public ITransformer Model { get; private set; }
        private PredictionEngine<I, O> predictionEngine;

        public PipelineBuilder(MLContext mlContext, IMLBuilder mlBuilder)
        {
            this.mlContext = mlContext;
            this.mlBuilder = mlBuilder;
        }

        public Pipeline<R, O> CreatePipeline<R>(IModifier<I, R> modifier) where R : class, new()
        {
            var pipeline = new Pipeline<R, O>(modifier, mlBuilder);
            linkedPipeline = pipeline;
            return pipeline;
        }

        public void PrintPipelineStructure()
        {
            linkedPipeline.PrintPipelineStructure();
        }

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            // Injecting some values inside the modifiers
            var nI = 0;
            foreach (var trainerIterable in GetTransformersOfPipeline<ITrainerIterable>())
            {
                nI = Math.Max(nI, trainerIterable.NumberOfIterations);
            }
            foreach (var totalNumberRequirement in GetTransformersOfPipeline<ITotalNumberRequirement>())
            {
                totalNumberRequirement.TotalCount = (int)(dataView.GetRowCount() * nI);
            }

            Model = linkedPipeline.GetPipeline(mlContext).Fit(dataView);
            trasformedDataView = Model.Transform(dataView);
            predictionEngine = mlContext.Model.CreatePredictionEngine<I, O>(Model);
        }

        public O Predict(I toPredict)
        {
            return predictionEngine.Predict(toPredict);
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
