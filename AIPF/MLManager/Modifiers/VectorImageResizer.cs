using Microsoft.ML;
using AIPF.Images;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace AIPF.MLManager.Modifiers
{
    public class VectorImageResizer : AbstractImageResizer
    {
        public VectorImageResizer(int originalWidth = 32, int originalHeight = 32, int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        : base(originalWidth, originalHeight, resizedHeight, resizedWidth, applyGrayScale) { }

        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = mlContext.Transforms.ConvertToImage(OriginalWidth, OriginalHeight, outputColumnName: "Image", inputColumnName: nameof(RawImage.Elements)); // , colorsPresent: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Blue

            if (ApplyGrayScale)
                pipeline = pipeline.Append(mlContext.Transforms.ConvertToGrayscale(outputColumnName: "GrayScaleResizedImage", inputColumnName: "Image"));                

            return pipeline.Append(mlContext.Transforms.ResizeImages(outputColumnName: "ResizedImage", ResizedWidth, ResizedHeight, inputColumnName: ApplyGrayScale ? "GrayScaleResizedImage" : "Image", ResizingKind.Fill))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: nameof(ProcessedImage.Pixels), inputColumnName: "ResizedImage", colorsToExtract: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Blue))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(RawImage.Digit)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));

        }
    }
}
