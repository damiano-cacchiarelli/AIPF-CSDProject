using AIPF.Images;
using AIPF.MLManager.Metrics;
using AIPF.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIPF
{
    public class Utils
    {
        public static List<VectorRawImage> ReadImageFromFile(string path, int skipLine = 0)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"The file does not exist: {path}");
            }
            ConsoleHelper.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<VectorRawImage> originalImages = new List<VectorRawImage>();

            ConsoleProgress consoleProgress = new ConsoleProgress("Generating Original Image");
            for (int i = skipLine; i < lines.Count; i += 33)
            {
                consoleProgress.Report((double)i/ lines.Count);
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
            ConsoleHelper.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<BitmapRawImage> originalImages = new List<BitmapRawImage>();

            for (int i = skipLine; i < lines.Count; i += 33)
            {
                ConsoleHelper.WriteLine($"Generating Original Image with lines {i} - {i + 32}");
                var digit = "-1";
                if (lines.Count > i + 32)
                {
                    digit = lines[i + 32];
                }
                originalImages.Add(new BitmapRawImage(lines.GetRange(i, 32), digit));
            }

            return originalImages;
        }

        public static void PrintPrediction(OutputImage predictedImage, int digit)
        {
            ConsoleHelper.WriteLine("");
            ConsoleHelper.WriteLine($"Actual: {digit}     Predicted probability:       zero:  {predictedImage.Digit[0]:0.####}");
            ConsoleHelper.WriteLine($"                                           one :  {predictedImage.Digit[1]:0.####}");
            ConsoleHelper.WriteLine($"                                           two:   {predictedImage.Digit[2]:0.####}");
            ConsoleHelper.WriteLine($"                                           three: {predictedImage.Digit[3]:0.####}");
            ConsoleHelper.WriteLine($"                                           four:  {predictedImage.Digit[4]:0.####}");
            ConsoleHelper.WriteLine($"                                           five:  {predictedImage.Digit[5]:0.####}");
            ConsoleHelper.WriteLine($"                                           six:   {predictedImage.Digit[6]:0.####}");
            ConsoleHelper.WriteLine($"                                           seven: {predictedImage.Digit[7]:0.####}");
            ConsoleHelper.WriteLine($"                                           eight: {predictedImage.Digit[8]:0.####}");
            ConsoleHelper.WriteLine($"                                           nine:  {predictedImage.Digit[9]:0.####}");
        }

        public static void PrintMetrics(List<MetricContainer> metrics)
        {
            ConsoleHelper.WriteLine("\n========= Metrics =========");
            metrics.ForEach(m => ConsoleHelper.WriteLine(m.ToString()));
            if (metrics.Count == 0) ConsoleHelper.WriteLine("No metrics available");
            ConsoleHelper.WriteLine("========= ------- =========\n");
        }
    }
}
