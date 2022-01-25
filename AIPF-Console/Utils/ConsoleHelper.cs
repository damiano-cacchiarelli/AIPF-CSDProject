using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AIPF.MLManager.EventQueue;
using AIPF.MLManager.Metrics;
using AIPF_Console.MNIST_example.Model;
using Spectre.Console;

namespace AIPF_Console.Utils
{
    class ConsoleHelper
    {
        public static void FitLoader()
        {
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),            // Task description
                        new ProgressBarColumn(),                // Progress bar
                        new PercentageColumn(),                 // Percentage
                        new SpinnerColumn(),  // Spinner
                    })
                .Start(ctx =>
                {
                    var random = new Random(DateTime.Now.Millisecond);
                    var task1 = ctx.AddTask("Preparing pipeline");
                    var task2 = ctx.AddTask("Fitting model", autoStart: false).IsIndeterminate();

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(10 * random.NextDouble());
                        Thread.Sleep(75);
                    }

                    task2.StartTask();
                    task2.IsIndeterminate(false);
                    while (!ctx.IsFinished)
                    {
                        task2.Increment(8 * random.NextDouble());
                        Thread.Sleep(75);
                    }
                });
        }

        public static async Task Loading(string taskName, string messageQueueId)
        {
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
                        var task1 = ctx.AddTask(taskName, maxValue: 1);
                        await foreach (var progress in MessageManager.IMessageQueue.DequeueAsync(messageQueueId, CancellationToken.None))
                        {
                            task1.Value = progress;
                            if (task1.IsFinished) break;
                        }
                    });
        }

        public static string MetricContainerToString(MetricContainer metricContainer)
        {
            var metrics = metricContainer.Metrics.Select(m => MetricOptionsToString(m));
            string line = string.Join("\n\t-- ", metrics);
            return $"{metricContainer.Name}\n\t-- {line}";
        }

        public static string MetricOptionsToString(MetricOptions metricOption)
        {
            string toString = $"{metricOption.Name}: {metricOption.Value}";
            toString += metricOption.IsBetterIfCloserTo != null ? $", is better if closer to { metricOption.IsBetterIfCloserTo}" : "";
            if (metricOption.Min != null && metricOption.Max != null)
            {
                toString += $" (minimum value = {metricOption.Min}, maximum value = {metricOption.Max})";
            }
            else
            {
                toString += metricOption.Min != null ? $" (minimum value = {metricOption.Min})" : "";
                toString += metricOption.Max != null ? $" (maximum value = {metricOption.Max})" : "";
            }
            return toString;
        }

        public static void PrintMetrics(List<MetricContainer> metricContainerList)
        {
            if (metricContainerList.Count == 0)
            {
                AnsiConsole.WriteLine("No available metrics.");
            }
            else
            {
                metricContainerList.ForEach(m => AnsiConsole.WriteLine(MetricContainerToString(m)));
            }
        }

        public static void PrintPrediction(OutputImage predictedImage, int digit)
        {
            AnsiConsole.WriteLine("");
            AnsiConsole.WriteLine($"Actual: {digit}     Predicted probability:       zero:  {predictedImage.Digit[0]:0.####}");
            AnsiConsole.WriteLine($"                                           one :  {predictedImage.Digit[1]:0.####}");
            AnsiConsole.WriteLine($"                                           two:   {predictedImage.Digit[2]:0.####}");
            AnsiConsole.WriteLine($"                                           three: {predictedImage.Digit[3]:0.####}");
            AnsiConsole.WriteLine($"                                           four:  {predictedImage.Digit[4]:0.####}");
            AnsiConsole.WriteLine($"                                           five:  {predictedImage.Digit[5]:0.####}");
            AnsiConsole.WriteLine($"                                           six:   {predictedImage.Digit[6]:0.####}");
            AnsiConsole.WriteLine($"                                           seven: {predictedImage.Digit[7]:0.####}");
            AnsiConsole.WriteLine($"                                           eight: {predictedImage.Digit[8]:0.####}");
            AnsiConsole.WriteLine($"                                           nine:  {predictedImage.Digit[9]:0.####}");
        }

    }
}
