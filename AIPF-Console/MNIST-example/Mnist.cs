using AIPF.MLManager;
using AIPF.MLManager.Metrics;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AIPF_Console.MNIST_example
{
    public abstract class Mnist : IExample
    {
        protected MLManager<VectorRawImage, OutputImage> mlManager;
        private List<VectorRawImage> rawImageDataList = null;

        private string name;
        public string Name { get => name; private set => name = value; }

        protected Mnist(string name)
        {
            Name = name;
            mlManager= new MLManager<VectorRawImage, OutputImage>(name);
        }

        protected abstract void CreatePipeline(int numberOfIterations);

        public async Task Metrics(int? numberOfElementsForEvaluate)
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
                var taskMetrics = mlManager.EvaluateAll(rawImageDataList.OrderBy(x => IExample.random.Next()).Take(numberOfElementsForEvaluate ?? rawImageDataList.Count));
                await ConsoleHelper.Loading("Evaluating model", $"{Name}Process#1");
                metrics = await taskMetrics;
            }

            ConsoleHelper.PrintMetrics(metrics);
        }

        public async Task Predict(PredictionMode predictionMode = PredictionMode.USER_VALUE, int error = 0)
        {

            AnsiConsole.Write(new Rule("[yellow]Predicting[/]").RuleStyle("grey").LeftAligned());

            var path = $"{IExample.Dir}/MNIST-example/Data/image_to_predict.txt";
            if (predictionMode == PredictionMode.USER_VALUE)
            {
                path = AnsiConsole.Ask<string>("Insert the path of the image to predict ", $"{IExample.Dir}/MNIST-example/Data/image_to_predict.txt");
            }

            VectorRawImage rawImageToPredict = MnistLoader.ReadImageFromFile(path)[0];

            if (predictionMode == PredictionMode.RANDOM_VALUE)
            {
                if (rawImageDataList == null)
                    rawImageDataList = MnistLoader.ReadImageFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);
                rawImageToPredict = rawImageDataList[IExample.random.Next(0, rawImageDataList.Count - 1)];
            }

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

        public async Task Train(int? numberOfIterations, int? numberOfElementsForTrain)
        {
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
            else if (!mlManager.Trained)
            {
                CreatePipeline(numberOfIterations ?? 100);

                var fittingTask = mlManager.Fit(rawImageDataList.OrderBy(x => IExample.random.Next()).Take(numberOfElementsForTrain ?? rawImageDataList.Count));
                await ConsoleHelper.Loading("Fitting pipeline", $"{Name}Process#1");
                await fittingTask;
            }

            AnsiConsole.WriteLine("Train complete!");
        }

        /* NOT WORK... Cursor exception caused by Bitmap! */
        /*
        public static async Task PredictUsingBitmapPipeline()
        {
            var rawImageDataList = MnistLoader.ReadBitmapFromFile($"{IExample.Dir}/MNIST-example/Data/optdigits_original_training.txt", 21);

            var mlMaster = new MLManager<BitmapRawImage, OutputImage>();
            mlMaster.CreatePipeline()
                .AddTransformer(new ProgressIndicator<BitmapRawImage>(@"Process#1"))
                .Append(new BitmapResizer())
                .Append(new SdcaMaximumEntropy(100));

            await mlMaster.Fit(rawImageDataList);
            var metrics = await mlMaster.EvaluateAll(rawImageDataList);
            ConsoleHelper.PrintMetrics(metrics);

            // Digit = 6
            BitmapRawImage rawImageToPredict = MnistLoader.ReadBitmapFromFile($"{IExample.Dir}/MNIST-example/Data/image_to_predict.txt")[0];
            OutputImage predictedImage = await mlMaster.Predict(rawImageToPredict);
            ConsoleHelper.PrintPrediction(predictedImage, 0);
        }*/

    }
}
