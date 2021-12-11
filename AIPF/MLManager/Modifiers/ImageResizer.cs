﻿using Microsoft.ML;
using AIPF.Images;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace AIPF.MLManager.Modifiers
{
    public class ImageResizer : AbstractImageResizer
    {
        public ImageResizer(int originalWidth = 32, int originalHeight = 32, int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        : base(originalWidth, originalHeight, resizedHeight, resizedWidth, applyGrayScale) { }

        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            IEstimator<ITransformer> pipeline = mlContext.Transforms.ResizeImages(outputColumnName: "ResizedImage", ResizedWidth, ResizedHeight, inputColumnName: "Elements")
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: nameof(ProcessedImage.Pixels), inputColumnName: "ResizedImage", colorsToExtract: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Red, scaleImage:1f/256)); //mlContext.Transforms.ConvertToImage(OriginalWidth, OriginalHeight, outputColumnName: "Image", inputColumnName: nameof(RawImage.Elements)); // , colorsPresent: Microsoft.ML.Transforms.Image.ImagePixelExtractingEstimator.ColorBits.Blue

            //           if (ApplyGrayScale)
            //               pipeline = pipeline.Append(mlContext.Transforms.ConvertToGrayscale(outputColumnName: "GrayScaleResizedImage", inputColumnName: "Image"));                

            return pipeline
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(RawImage.Digit)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));

        }
    }
}
