using AIPF.MLManager.Metrics;
using AIPF_Console.MNIST_example.Model;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public class Utils
    {

        public static List<VectorRawImage> ReadImageFromFile(string path, int skipLine = 0)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"The file does not exist: {path}");
            }
            //ConsoleHelper.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<VectorRawImage> originalImages = new List<VectorRawImage>();

            //ConsoleProgress consoleProgress = new ConsoleProgress("Generating Original Image");
            for (int i = skipLine; i < lines.Count; i += 33)
            {
                //consoleProgress.Report((double)i/ lines.Count);
                //ConsoleHelper.WriteLine($"Generating Original Image with lines {i} - {i + 32}");
                var digit = "-1";
                if (lines.Count > i + 32)
                {
                    digit = lines[i + 32];
                }
                originalImages.Add(new VectorRawImage(lines.GetRange(i, 32), digit));
            }

            return originalImages;
        }

        public static List<BitmapRawImage> ReadBitmapFromFile(string path, int skipLine = 0)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"The file does not exist: {path}");
            }
            //ConsoleHelper.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<BitmapRawImage> originalImages = new List<BitmapRawImage>();

            for (int i = skipLine; i < lines.Count; i += 33)
            {
                //ConsoleHelper.WriteLine($"Generating Original Image with lines {i} - {i + 32}");
                var digit = "-1";
                if (lines.Count > i + 32)
                {
                    digit = lines[i + 32];
                }
                originalImages.Add(new BitmapRawImage(lines.GetRange(i, 32), digit));
            }

            return originalImages;
        }

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

        public static string MetricContainerToString(MetricContainer metricContainer)
        {
            var metrics = metricContainer.Metrics.Select( m => MetricOptionsToString(m));
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
