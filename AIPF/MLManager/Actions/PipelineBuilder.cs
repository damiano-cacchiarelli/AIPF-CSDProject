using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;

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

        /*
        public void PrintPipelineStructure()
        {
            linkedPipeline.PrintPipelineStructure();
        }
        */

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            if (linkedPipeline == null) 
                throw new Exception("You must create a pipeline before");

            // Injecting some values inside the modifiers
            var nI = 1;
            foreach (var trainerIterable in linkedPipeline.GetTransformersOfPipeline<ITrainerIterable>())
            {
                nI = Math.Max(nI, trainerIterable.NumberOfIterations);
            }
            foreach (var totalNumberRequirement in linkedPipeline.GetTransformersOfPipeline<ITotalNumberRequirement>())
            {
                var f = mlContext.Data.CreateEnumerable<I>(dataView, reuseRowObject: true).Count();
                //(dataView.GetRowCount() ?? 1) 
                totalNumberRequirement.TotalCount = (int)(f * nI);
            }

            linkedPipeline.GetModificators().ForEach(m => m.Begin());
            Model = linkedPipeline.GetPipeline(mlContext).Fit(dataView);
            trasformedDataView = Model.Transform(dataView);
            predictionEngine = mlContext.Model.CreatePredictionEngine<I, O>(Model);
            linkedPipeline.GetModificators().ForEach(m => m.End());
        }

        public O Predict(I toPredict)
        {
            if (predictionEngine == null)
                throw new Exception("You must create and execute a pipeline before");

            return predictionEngine.Predict(toPredict);
        }

        public List<MetricContainer> Evaluate(IDataView dataView, out IDataView transformedDataView)
        {
            List<MetricContainer> metrics = new List<MetricContainer>();
            Execute(dataView, out transformedDataView);

            foreach (var evaluable in linkedPipeline.GetTransformersOfPipeline<IEvaluable>())
            {
                var m = evaluable.Evaluate(mlContext, transformedDataView);

                if (m != null)
                    metrics.Add(m);
            }

            return metrics;
        }
    }
}
