using Microsoft.ML;
using AIPF.Images;
using System;

namespace AIPF.MLManager
{
    public class Pipeline<T> where T : class, new()
    {
        private MLContext mlContext;
        private Action<IEstimator<ITransformer>> UpdatePipeline;
        private IEstimator<ITransformer> pipeline;

        public Pipeline(MLContext mlContext, Action<IEstimator<ITransformer>> UpdatePipeline, IEstimator<ITransformer> pipeline)
        {
            this.mlContext = mlContext;
            this.pipeline = pipeline;
            this.UpdatePipeline = UpdatePipeline;
            UpdatePipeline?.Invoke(pipeline);
        }

        public Pipeline<R> Append<R>(IModifier<T, R> modifier) where R : class, new()
        {
            var pipeline = modifier.GetPipeline(mlContext);
            if (this.pipeline == null) this.pipeline = pipeline;
            else this.pipeline = this.pipeline.Append(pipeline);
            return new Pipeline<R>(mlContext, UpdatePipeline, this.pipeline);
        }

        public Pipeline<OutputImage> AddMlAlgorithm(int numberOfIteration = 10)
        {
            if (pipeline == null)
                throw new Exception("Pipeline required");

            var trainer = mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: numberOfIteration);
            pipeline = pipeline.Append(trainer)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(nameof(OutputImage.Digit), "Label"));

            return new Pipeline<OutputImage>(mlContext, UpdatePipeline, pipeline);
        }
    }
}
