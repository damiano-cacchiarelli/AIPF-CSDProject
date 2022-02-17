using System;
using System.Collections.Generic;
using AIPF_Console.MNIST_example.Model;
using Microsoft.ML;

namespace AIPF_Console.MNIST_example.Modifiers
{
    public class CustomImageResizer : AbstractImageResizer<VectorRawImage, float[], ProcessedImage>
    {

        public CustomImageResizer(int resizedWidth = 8, int resizedHeight = 8)
        : base(resizedHeight, resizedWidth, false) { }

        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            Action<VectorRawImage, ProcessedImage> mapping = (input, output) =>
            {
                output.Pixels = CustomResize(input);
                output.Digit = input.Digit;
            };
            return mlContext.Transforms.CustomMapping(mapping, contractName: null)
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(VectorRawImage.Digit)))
                .Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));
        }

        private float[] CustomResize(VectorRawImage rawImage)
        {
            List<float> list = new List<float>();

            int magicValueWidth = VectorRawImage.Width / ResizedWidth;
            int magicValueHeight = VectorRawImage.Height / ResizedHeight;

            for (int j = 0; j < VectorRawImage.Width; j += magicValueWidth)
            {
                for (int i = 0; i < VectorRawImage.Height; i += magicValueHeight)
                {
                    float sum = 0;
                    for (int x = 0; x < magicValueWidth; x++)
                    {
                        for (int y = 0; y < magicValueHeight; y++)
                        {
                            sum += rawImage.Elements[i + x + ((j + y) * VectorRawImage.Width)];
                        }
                    }
                    list.Add(sum);
                }
            }
            return list.ToArray();
        }
    }
}
