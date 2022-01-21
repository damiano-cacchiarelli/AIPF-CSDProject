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

        public void Metrics()
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
                metrics = mlManager.EvaluateAll(rawImageDataList);
            }

            ConsoleHelper.PrintMetrics(metrics);
        }

        public void Predict()
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
                predictedImage = mlManager.Predict(rawImageToPredict);
                
            }
            ConsoleHelper.PrintPrediction(predictedImage, 0);
        }

        public void Train() { }

        async Task IExample.Train2()
        {
            rawImageDataList = MnistLoader.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);
            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = rawImageDataList };
                using (var streamReader = RestService.PostStream("train", fitBody))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var message = await streamReader.ReadLineAsync();
                        Console.WriteLine($"Received price update: {message}");
                    }
                }
            }
            else
            {
                //var messageQueue = new MessageQueue<int>();
                mlManager.CreatePipeline()
                    //.AddTransformer(new ProgressIndicator<VectorRawImage>(@"Process#1", messageQueue))
                    // Using our custom image resizer
                    //.Append(new CustomImageResizer())
                    // OR using the ml.net default ResizeImages method
                    .AddTransformer(new VectorImageResizer())
                    .Append(new SdcaMaximumEntropy(90))
                    .Build();

                mlManager.Fit(rawImageDataList, out IDataView transformedDataView);

                /*
                new Thread(() => { mlManager.Fit(rawImageDataList, out IDataView transformedDataView); AnsiConsole.WriteLine("End fit!"); }).Start();

                await AnsiConsole.Progress()
                    .Columns(new ProgressColumn[]
                        {
                                            new TaskDescriptionColumn(),            // Task description
                                            new ProgressBarColumn(),                // Progress bar
                                            new PercentageColumn(),                 // Percentage
                                            new SpinnerColumn(),  // Spinner
                        })
                    .StartAsync(async ctx =>
                    {
                        var random = new Random(DateTime.Now.Millisecond);
                        var task1 = ctx.AddTask("Fitting pipeline", maxValue: 178200);
                        await foreach (var message in messageQueue.DequeueAsync(@"Process#1", CancellationToken.None))
                        {
                            task1.Increment(1);
                            //if (message >= 178200) break;
                        }
                    });
                */
            }
            ConsoleHelper.FitLoader();
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
