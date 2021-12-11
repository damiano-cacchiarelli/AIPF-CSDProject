using System;
using System.Collections.Generic;
using Microsoft.ML;
using AIPF.Images;

namespace AIPF.MLManager.Modifiers
{
    public class CustomImageResizer : AbstractImageResizer
    {
        public CustomImageResizer(int originalWidth = 32, int originalHeight = 32, int resizedWidth = 8, int resizedHeight = 8)
        : base(originalWidth, originalHeight, resizedHeight, resizedWidth, false) { }

        // p = [ t1, t1 ... tn ]
        public override IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            Action<RawImage, ProcessedImage> mapping = (input, output) =>
            {
                output.Pixels = CustomResize(input);
                output.Digit = input.Digit;
                /*
                Console.Write("\n");
                foreach (var item in output.Pixels)
                {
                    Console.Write(item+",");
                }
                Console.Write("\ndigit:"+input.Digit+"\n");
                */
            };
            return mlContext.Transforms.CustomMapping(mapping, contractName: null);
            //.Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "Label", inputColumnName: nameof(RawImage.Digit)))
            //.Append(mlContext.Transforms.Concatenate(outputColumnName: "Features", nameof(ProcessedImage.Pixels)));
        }

        private float[] CustomResize(IRawImage rawImage)
        {
            List<float> list = new List<float>();
            /*for (int j = 0; j < 32; j += 4)
            {
                for (int i = 0; i < 32; i += 4)
                {
                    float sum = rawImage.Elements[i + j * 32];
                    sum += rawImage.Elements[i + 1 + j * 32];
                    sum += rawImage.Elements[i + 2 + j * 32];
                    sum += rawImage.Elements[i + 3 + j * 32];

                    sum += rawImage.Elements[i + (j + 1) * 32];
                    sum += rawImage.Elements[i + 1 + (j + 1) * 32];
                    sum += rawImage.Elements[i + 2 + (j + 1) * 32];
                    sum += rawImage.Elements[i + 3 + (j + 1) * 32];

                    sum += rawImage.Elements[i + (j + 2) * 32];
                    sum += rawImage.Elements[i + 1 + (j + 2) * 32];
                    sum += rawImage.Elements[i + 2 + (j + 2) * 32];
                    sum += rawImage.Elements[i + 3 + (j + 2) * 32];

                    sum += rawImage.Elements[i + (j + 3) * 32];
                    sum += rawImage.Elements[i + 1 + (j + 3) * 32];
                    sum += rawImage.Elements[i + 2 + (j + 3) * 32];
                    sum += rawImage.Elements[i + 3 + (j + 3) * 32];
                    list.Add(sum);
                }
            }*/
            return list.ToArray();
        }
    }
}
