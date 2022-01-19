﻿using AIPF.MLManager;
using AIPF.MLManager.Metrics;
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
                rawImageDataList = Utils.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);

            List<MetricContainer> metrics;
            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = "MNIST", Data = rawImageDataList };
                metrics = Utils.MetricsRestCall(fitBody).Result;
            }
            else
            {
                metrics = mlManager.EvaluateAll(rawImageDataList);
            }

            Utils.PrintMetrics(metrics);
        }

        public void Predict()
        {
            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/image_to_predict.txt")[0];
            OutputImage predictedImage;
            if (Program.REST)
            {
                predictedImage = Utils.PredictRestCall<OutputImage>("MNIST", rawImageToPredict).Result;
            }
            else
            {
                predictedImage = mlManager.Predict(rawImageToPredict);
                
            }
            Utils.PrintPrediction(predictedImage, 0);
        }

        public void Train()
        {
            rawImageDataList = Utils.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);
            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = "MNIST", Data = rawImageDataList };
                Utils.TrainRestCall(fitBody);
            }
            else
            {
                mlManager.CreatePipeline()
                    //.AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                    // Using our custom image resizer
                    //.Append(new CustomImageResizer())
                    // OR using the ml.net default ResizeImages method
                    .AddTransformer(new VectorImageResizer())
                    .Append(new SdcaMaximumEntropy(3))
                    .Build();

                mlManager.Fit(rawImageDataList, out IDataView transformedDataView);
            }
            Utils.FitLoader();
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
