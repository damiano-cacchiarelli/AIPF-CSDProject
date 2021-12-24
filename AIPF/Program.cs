using Microsoft.ML;
using System.IO;
using System.Linq;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using AIPF.Images;
using System;
using AIPF.Data;
using AIPF.MLManager.Modifiers.Date;
using AIPF.Models.Taxi;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using System.Collections.Generic;

namespace AIPF
{
    class Program
    {
        static void Main(string[] args)
        {
            //PredictUsingVectorPipeline();
            //PredictUsingBitmapPipeline();
            //PredictUsingMorePipeline();

            //TaxiFarePrediction();
            Example();
        }

        private static void Example()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
            mlManager.CreatePipeline()
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => { Console.WriteLine(i.Distance); return i.Distance >= 0 && i.Distance <= 0.5; })
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, PredictedFareAmount>($"{dir}/Data/TaxiFare/Onnx/skl_hubReg.onnx"))
                .Build();
            var data = new RawStringTaxiFare[] { };
            mlManager.Fit(data, out var dataView);
            var prediction = mlManager.Predict(new RawStringTaxiFare()
            {
                DateAsString = "2010-01-05 16:52:16 UTC",
                X1 = -74.016048f,
                Y1 = 40.711303f,
                X2 = -73.979268f,
                Y2 = 40.782004f,
                PassengersCount = 1,
                // FareAmount = 16.9
            });
            if(prediction != null) Console.WriteLine(prediction.FareAmount[0]);
        }

        /**
        private static void TaxiFarePrediction()
        {

            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();

            mlManager.CreatePipeline(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute));


            mlManager.CreatePipeline(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Append(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, PredictedFareAmount>($"{dir}/Data/TaxiFare/Onnx/skl_hubReg.onnx"));

            //Load csv data
            //var data = mlManager.MlLoader.Load($"{dir}/Data/TaxiFare/train_mini.csv");
            var data = new RawStringTaxiFare[] { };
            mlManager.Fit(data, out IDataView transformedDataView);
            var prediction = mlManager.Predict(new RawStringTaxiFare()
            {
                DateAsString = "2010-01-05 16:52:16 UTC",
                X1 = -74.016048f,
                X2 = 40.711303f,
                Y1 = -73.979268f,
                Y2 = 40.782004f,
                PassengersCount = 1,
                // FareAmount = 16.9
            });
            Console.WriteLine(prediction.FareAmount[0]);
        }

        static void PredictUsingVectorPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<VectorRawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/Data/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);
        }

        static void PredictUsingBitmapPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<BitmapRawImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressIndicator<BitmapRawImage>(@"Process#1"))
                .Append(new BitmapResizer())
                .Append(new SdcaMaximumEntropy(100));

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            var metrics = mlMaster.EvaluateAll();
            Utils.PrintMetrics(metrics);

            // Digit = 6
            BitmapRawImage rawImageToPredict = Utils.ReadBitmapFromFile($"{dir}/Data/MNIST/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);
        }

        static void PredictUsingMorePipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/optdigits_original_training.txt", 21);

            // Data pre-processing pipeline: 32x32 => 8x8 images (We don't use this pipeline to predict, only to get processed data!!)
            var preProcessingMlMaster = new MLManager<VectorRawImage, ProcessedImage>();
            preProcessingMlMaster.CreatePipeline(new ProgressIndicator<VectorRawImage>(@"Process#1_ResizingImage"))
                .Append(new CustomImageResizer());
            preProcessingMlMaster.Fit(rawImageDataList, out IDataView transformedDataView);
            transformedDataView.Preview();
            var processedImages = preProcessingMlMaster.GetEnumerable(transformedDataView);

            // Train & predict Pipeline
            var mlMaster = new MLManager<ProcessedImage, OutputImage>();
            mlMaster.CreatePipeline(new ProgressPercentageIndicator<ProcessedImage>(@"Process#2"))
                .Append(new SdcaMaximumEntropy(1));
            mlMaster.Fit(processedImages, out _);

            ProcessedImage rawImageToPredict = new ProcessedImage()
            {
                // Digit = 7
                Pixels = new float[] { 0, 0, 0, 0, 14, 13, 1, 0, 0, 0, 0, 5, 16, 16, 2, 0, 0, 0, 0, 14, 16, 12, 0, 0, 0, 1, 10, 16, 16, 12, 0, 0, 0, 3, 12, 14, 16, 9, 0, 0, 0, 0, 0, 5, 16, 15, 0, 0, 0, 0, 0, 4, 16, 14, 0, 0, 0, 0, 0, 1, 13, 16, 1, 0 }
            };

            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 7);
        }
        */
    }
}
