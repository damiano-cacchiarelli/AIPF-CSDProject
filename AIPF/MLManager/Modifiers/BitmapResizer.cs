using Microsoft.ML;
using AIPF.Images;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;
using System.Drawing;

namespace AIPF.MLManager.Modifiers
{
    public class BitmapResizer : AbstractImageResizer<BitmapRawImage, Bitmap, ProcessedImage>
    {
        public BitmapResizer(int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        : base(resizedHeight, resizedWidth, applyGrayScale) { }

        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = null;

            if (ApplyGrayScale)
                pipeline = mlContext.Transforms.ConvertToGrayscale(outputColumnName: "GrayScaleResizedImage", inputColumnName: nameof(BitmapRawImage.Elements));

            var tempPipeline = mlContext.Transforms.ResizeImages(outputColumnName: "ResizedImage", ResizedWidth, ResizedHeight, inputColumnName: ApplyGrayScale ? "GrayScaleResizedImage" : nameof(BitmapRawImage.Elements));
            if  (pipeline == null) pipeline = tempPipeline;
            else pipeline = pipeline.Append(tempPipeline);

            return pipeline.Append(mlContext.Transforms.ExtractPixels(outputColumnName: nameof(ProcessedImage.Pixels), inputColumnName: "ResizedImage", colorsToExtract: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Red, scaleImage: 1f / 256))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(BitmapRawImage.Digit)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));
        }
    }
}
