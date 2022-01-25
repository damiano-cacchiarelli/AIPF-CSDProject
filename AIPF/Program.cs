using Microsoft.ML;
using System.IO;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers;
using System;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Actions.Filters;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AIPF
{
    class Program
    {
        public static ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("NonHostConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        });

        static void Main(string[] args)
        {
            //PredictUsingVectorPipeline();
            //PredictUsingBitmapPipeline();
            //PredictUsingMorePipeline();
            //TaxiFarePrediction();
        }
/*
        private static void TaxiFarePrediction()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{dir}/Data/TaxiFare/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn2<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{dir}/Data/TaxiFare/Onnx/skl_pca_linReg.onnx"))
                .Build();
            var data = new RawStringTaxiFare[] { };
            mlManager.Fit(data, out var dataView);
            dataView.Preview();
            var prediction = mlManager.Predict(new RawStringTaxiFare()
            {
                DateAsString = "2011-08-18 00:35:00 UTC",
                X1 = -73.982738f,
                Y1 = 40.76127f,
                X2 = -73.991242f,
                Y2 = 40.750562f,
                PassengersCount = 2,
                // FareAmount = 5.7
            });
            // hubReg = 6.7486925
            // linReg = 7.584161
            // pca_hubReg = 6.7486873
            // pca_linReg = 7.58416
            if (prediction != null) Console.WriteLine(prediction.FareAmount[0]);


            var metrics = mlManager.EvaluateAll(mlManager.Loader.LoadFile($"{dir}/Data/TaxiFare/train_mini.csv"));

            /*
            var loadedData = new MLContext().Data.CreateEnumerable<RawStringTaxiFare>(mlManager.Loader.LoadFile($"{dir}/Data/TaxiFare/train_mini.csv"),
                reuseRowObject: true);
            foreach (var item in loadedData)
            {
                var prediction2 = mlManager.Predict(item);
                if (prediction2 != null) Console.WriteLine($"actual:{item.FareAmount}, prediction:{prediction2.FareAmount[0]}");
            }
            */
//        }*/
/*
        static void PredictUsingVectorPipeline()
        {
            string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var rawImageDataList = Utils.ReadImageFromFile($"{dir}/Data/MNIST/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<VectorRawImage, OutputImage>();
            mlMaster.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                // Using our custom image resizer
                //.Append(new CustomImageResizer())
                // OR using the ml.net default ResizeImages method
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(3))
                .Build();

            mlMaster.Fit(rawImageDataList, out IDataView transformedDataView);

            //var metrics = mlMaster.EvaluateAll();
            //Utils.PrintMetrics(metrics);

            // Digit = 6
            VectorRawImage rawImageToPredict = Utils.ReadImageFromFile($"{dir}/Data/MNIST/image_to_predict.txt").First();
            OutputImage predictedImage = mlMaster.Predict(rawImageToPredict);
            Utils.PrintPrediction(predictedImage, 0);

            var metrics = mlMaster.EvaluateAll(new MLContext().Data.LoadFromEnumerable(rawImageDataList));
        }
*/

        /*
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
