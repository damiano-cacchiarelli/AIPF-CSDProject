using AIPF.MLManager.Actions.Modifiers;
using AIPF.MLManager.Metrics;
using AIPF.Utilities;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AIPF.MLManager.Actions
{
    public class PipelineBuilder<I, O> : ITransformerAction<I, O> where I : class, new() where O : class, new()
    {
        public static readonly ActivitySource source = new ActivitySource("PipelineBuilder");

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

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            if (linkedPipeline == null) 
                throw new Exception("You must create a pipeline before");

            using var activity = source.StartActivity($"Transformation pipeline");
            //using var activity = Activity.Current.Source.StartActivity($"Execute Transformation pipeline");
            activity?.AddTag("pipeline_builder.type", typeof(PipelineBuilder<I, O>).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.processed_elements", MLUtils.GetDataViewLength<I>(mlContext, dataView));
            activity?.AddTag("pipeline_builder.input.type", typeof(I).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.output.type", typeof(O).ToGenericTypeString());

            InjectValuesOnPipeline(dataView);

            linkedPipeline.GetModificators().ForEach(m => m.Begin());
            Model = linkedPipeline.GetPipeline(mlContext).Fit(dataView);
            trasformedDataView = Model.Transform(dataView);
            predictionEngine = mlContext.Model.CreatePredictionEngine<I, O>(Model);
            linkedPipeline.GetModificators().ForEach(m => m.End());

            activity?.AddEvent(new ActivityEvent("Execution ended"));
        }

        public O Predict(I toPredict)
        {
            if (predictionEngine == null)
                throw new Exception("You must create and execute a pipeline before");

            using var activity = source.StartActivity($"Predict pipeline");
            activity?.AddTag("pipeline_builder.input.type", typeof(I).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.output.type", typeof(O).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.type", typeof(PipelineBuilder<I, O>).ToGenericTypeString());
            return predictionEngine.Predict(toPredict);
        }

        public List<MetricContainer> Evaluate(IDataView dataView, out IDataView transformedDataView)
        {
            using var activity = source.StartActivity($"Evaluate pipeline");
            activity?.AddTag("pipeline_builder.input.type", typeof(I).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.output.type", typeof(O).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.type", typeof(PipelineBuilder<I, O>).ToGenericTypeString());
            activity?.AddTag("pipeline_builder.processed_elements", MLUtils.GetDataViewLength<I>(mlContext, dataView));

            List<MetricContainer> metrics = new List<MetricContainer>();

            InjectValuesOnPipeline(dataView);
            linkedPipeline.GetModificators().ForEach(m => m.Begin());
            transformedDataView = Model.Transform(dataView);

            foreach (var evaluable in linkedPipeline.GetTransformersOfPipeline<IEvaluable>())
            {
                var m = evaluable.Evaluate(mlContext, transformedDataView);

                if (m != null)
                    metrics.Add(m);
            }

            linkedPipeline.GetModificators().ForEach(m => m.End());

            return metrics;
        }

        private void InjectValuesOnPipeline(IDataView dataView)
        {
            var nI = 1;
            foreach (var trainerIterable in linkedPipeline.GetTransformersOfPipeline<ITrainerIterable>())
            {
                nI = Math.Max(nI, trainerIterable.NumberOfIterations);
            }

            long count = MLUtils.GetDataViewLength<I>(mlContext, dataView);

            foreach (var totalNumberRequirement in linkedPipeline.GetTransformersOfPipeline<ITotalNumberRequirement>())
            {
                totalNumberRequirement.TotalCount = (int)(count * nI);
            }
        }
    }
}
