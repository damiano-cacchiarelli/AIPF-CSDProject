using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.MNIST_example.Modifiers;
using Microsoft.ML;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AIPF_Console.MNIST_example
{
    public class Mnist : IExample
    {
        private string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        private MLManager<VectorRawImage, OutputImage> mlManager = new MLManager<VectorRawImage, OutputImage>();

        private List<VectorRawImage> rawImageDataList = null;

        protected Mnist()
        {
        }
        public static IExample Start()
        {
            return new Mnist();
        }

        public string GetName()
        {
            return "MNIST";
        }

        public void Metrics()
        {
            if (rawImageDataList == null)
                rawImageDataList = Utils.ReadImageFromFile($"{dir}/MNIST-example/Data/optdigits_original_training.txt", 21);

            var metrics = mlManager.EvaluateAll(new MLContext().Data.LoadFromEnumerable(rawImageDataList));

            if (metrics.Count == 0 || true)
            {
                AnsiConsole.WriteLine("No available metrics.");
            }
            else
            {
                metrics.ForEach(m => AnsiConsole.WriteLine(m.ToString()));
            }


        }

        public void Predict()
        {
            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/MNIST-example/Data/image_to_predict.txt")[0];
            OutputImage predictedImage = mlManager.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);

        }

        public void Train()
        {
            rawImageDataList = Utils.ReadImageFromFile($"{dir}/MNIST-example/Data/optdigits_original_training.txt", 21);

            mlManager.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3))
                .Build();

            mlManager.Fit(rawImageDataList, out IDataView transformedDataView);
        }
/*
        static void PredictUsingBitmapPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<BitmapRawImage, OutputImage>();
            mlMaster.CreatePipeline()
                .AddTransformer(new ProgressIndicator<BitmapRawImage>(@"Process#1"))
                .Append(new BitmapResizer())
                .Append(new SdcaMaximumEntropy(100));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll(transformedDataView);
            //Utils.PrintMetrics(metrics);

            // Digit = 6
            BitmapRawImage rawImageToPredict = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/image_to_predict.txt")[0];
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            //Utils.PrintPrediction(predictedImage, 0);
        }*/
    }
}
