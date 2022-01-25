using AIPF.MLManager;
using AIPF.MLManager.EventQueue;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.MNIST_example.Modifiers;
using AIPF_Console.Utils;
using Microsoft.ML;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF_Console.MNIST_example
{
    public class Mnist : IExample
    {
        private MLManager<VectorRawImage, OutputImage> mlManager = new MLManager<VectorRawImage, OutputImage>();

        private List<VectorRawImage> rawImageDataList = null;
        public string Name => "MNIST";

        protected Mnist()
        {
        }

        public static IExample Start()
        {
            return new Mnist();
        }

        public async Task Metrics()
        {
            if (rawImageDataList == null)
                rawImageDataList = MnistLoader.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);

            List<MetricContainer> metrics;
            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = rawImageDataList };
                metrics = RestService.Put<List<MetricContainer>>("metrics", fitBody).Result;
            }
            else
            {
                metrics = await mlManager.EvaluateAll(rawImageDataList);
            }

            ConsoleHelper.PrintMetrics(metrics);
        }

        public async Task Predict()
        {
            // Digit = 6
            VectorRawImage rawImageToPredict = MnistLoader.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/image_to_predict.txt")[0];
            OutputImage predictedImage;
            if (Program.REST)
            {
                predictedImage = RestService.Put<OutputImage>($"predict/{Name}", rawImageToPredict).Result;
            }
            else
            {
                predictedImage = await mlManager.Predict(rawImageToPredict);
                
            }
            ConsoleHelper.PrintPrediction(predictedImage, 0);
        }

        public async Task Train() {

            rawImageDataList = MnistLoader.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);
            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = rawImageDataList };
                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                        {
                            new TaskDescriptionColumn(),    // Task description
                            new ProgressBarColumn(),        // Progress bar
                            new PercentageColumn(),         // Percentage
                            new SpinnerColumn(),            // Spinner
                        })
                    .StartAsync(async ctx =>
                    {
                        var task1 = ctx.AddTask("Fitting pipeline", maxValue: 1);
                        using (var streamReader = new StreamReader(await RestService.PostStream("train", fitBody)))
                        {
                            while (!streamReader.EndOfStream)
                            {
                                var progress = await streamReader.ReadLineAsync();
                                task1.Value = double.Parse(progress);
                                ctx.Refresh();
                            }
                        }
                    });

            }
            else
            {

                mlManager.CreatePipeline()
                    .AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1"))
                    .Append(new VectorImageResizer())
                    .Append(new SdcaMaximumEntropy(200))
                    .Build();

                var fittingTask = mlManager.Fit(rawImageDataList);
                await ConsoleHelper.Loading("Fitting pipeline", @"Process#1");
                await fittingTask;
            }

            AnsiConsole.WriteLine("Train complete!");
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
