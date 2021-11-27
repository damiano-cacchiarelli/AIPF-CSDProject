using AIPF.Images;
using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public class SdcaMaximumEntropy : IModifier<ProcessedImage, OutputImage>
    {
        private int numberOfIteration;

        public SdcaMaximumEntropy(int numberOfIteration = 10)
        {
            this.numberOfIteration = numberOfIteration;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                labelColumnName: "Label",
                featureColumnName: "Features",
                maximumNumberOfIterations: numberOfIteration)
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(nameof(OutputImage.Digit), "Label"));
        }
    }
}
