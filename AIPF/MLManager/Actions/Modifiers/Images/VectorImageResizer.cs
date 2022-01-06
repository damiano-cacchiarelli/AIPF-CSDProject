using AIPF.Models.Images;
using Microsoft.ML;

namespace AIPF.MLManager.Modifiers
{
    public class VectorImageResizer : AbstractImageResizer<VectorRawImage, float[], ProcessedImage>
    {
        public VectorImageResizer(int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        : base(resizedHeight, resizedWidth, applyGrayScale) { }

        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            var pipeline = mlContext.Transforms.ConvertToImage(VectorRawImage.Width, VectorRawImage.Height, outputColumnName: "Image", inputColumnName: nameof(VectorRawImage.Elements), colorsPresent: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Blue);

            if (ApplyGrayScale)
                pipeline.Append(mlContext.Transforms.ConvertToGrayscale(outputColumnName: "GrayScaleResizedImage", inputColumnName: "ResizedImage"));

            return pipeline.Append(mlContext.Transforms.ResizeImages(outputColumnName: "ResizedImage", ResizedWidth, ResizedHeight, inputColumnName: ApplyGrayScale ? "GrayScaleResizedImage" : "Image"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: nameof(ProcessedImage.Pixels), inputColumnName: "ResizedImage", colorsToExtract: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Blue))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(VectorRawImage.Digit)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));
        }
    }
}
