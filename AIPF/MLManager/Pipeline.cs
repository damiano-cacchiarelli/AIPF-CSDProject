using Microsoft.ML;
using AIPF.Images;
using System;
using AIPF.MLManager.Modifiers;

namespace AIPF.MLManager
{
    public class Pipeline<T> : IPipeline where T : class, new()
    {
        private MLContext mlContext;
        private Action<IPipeline> UpdatePipeline;
        private IEstimator<ITransformer> pipeline;
        private IModificator modificator;

        public Pipeline(MLContext mlContext, Action<IPipeline> UpdatePipeline, IModificator modificator, IEstimator<ITransformer> pipeline)
        {
            this.mlContext = mlContext;
            this.pipeline = pipeline;
            this.UpdatePipeline = UpdatePipeline;
            this.modificator = modificator;
            UpdatePipeline?.Invoke(this);
        }

        public Pipeline<R> Append<R>(IModifier<T, R> modifier) where R : class, new()
        {
            var pipeline = modifier.GetPipeline(mlContext);
            if (this.pipeline == null) this.pipeline = pipeline;
            else this.pipeline = this.pipeline.Append(pipeline);
            return new Pipeline<R>(mlContext, UpdatePipeline, modifier, this.pipeline);
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

            return new Pipeline<OutputImage>(mlContext, UpdatePipeline, null, pipeline);
        }

        public IModificator GetModificator()
        {
            return modificator;
        }

        public IEstimator<ITransformer> GetPipeline()
        {
            return pipeline;
        }
    }
}
