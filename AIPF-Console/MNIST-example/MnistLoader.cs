using AIPF_Console.MNIST_example.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIPF_Console.MNIST_example
{
    public class MnistLoader
    {
        public static List<VectorRawImage> ReadImageFromFile(string path, int skipLine = 0)
        {
            return ReadFromFile(path, (l, d) => new VectorRawImage(l, d), skipLine);
        }

        public static List<BitmapRawImage> ReadBitmapFromFile(string path, int skipLine = 0)
        {
            return ReadFromFile(path, (l, d) => new BitmapRawImage(l,d), skipLine);
        }

        private static List<T> ReadFromFile<T>(string path, Func<List<string>, string, T> constructor, int skipLine = 0)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"The file does not exist: {path}");
            }
            //ConsoleHelper.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<T> originalImages = new List<T>();

            for (int i = skipLine; i < lines.Count; i += 33)
            {
                //ConsoleHelper.WriteLine($"Generating Original Image with lines {i} - {i + 32}");
                var digit = "-1";
                if (lines.Count > i + 32)
                {
                    digit = lines[i + 32];
                }
                originalImages.Add(constructor.Invoke(lines.GetRange(i, 32), digit));
            }

            return originalImages;
        }
    }
}
