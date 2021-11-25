using AIPF.Images;
using System;
using System.Collections.Generic;
using System.IO;

namespace AIPF
{
    public class Utils
    {
        public static List<RawImage> ReadImageFromFile(string path, int skipLine = 0)
        {
            if (!File.Exists(path))
            {
                throw new Exception($"The file does not exist: {path}");
            }
            Console.WriteLine($"Reading File {path}");
            List<string> lines = new List<string>(File.ReadAllLines(path));
            List<RawImage> originalImages = new List<RawImage>();

            for (int i = skipLine; i < lines.Count; i += 33)
            {
                Console.WriteLine($"Generating Original Image with lines {i} - {i + 32}");
                var digit = "-1";
                if (lines.Count > i + 32)
                {
                    digit = lines[i + 32];
                }
                originalImages.Add(new RawImage(lines.GetRange(i, 32), digit));
            }

            return originalImages;
        }

        public static void PrintPrediction(OutputImage predictedImage, int digit)
        {
            Console.WriteLine("");
            Console.WriteLine($"Actual: {digit}     Predicted probability:       zero:  {predictedImage.Digit[0]:0.####}");
            Console.WriteLine($"                                           one :  {predictedImage.Digit[1]:0.####}");
            Console.WriteLine($"                                           two:   {predictedImage.Digit[2]:0.####}");
            Console.WriteLine($"                                           three: {predictedImage.Digit[3]:0.####}");
            Console.WriteLine($"                                           four:  {predictedImage.Digit[4]:0.####}");
            Console.WriteLine($"                                           five:  {predictedImage.Digit[5]:0.####}");
            Console.WriteLine($"                                           six:   {predictedImage.Digit[6]:0.####}");
            Console.WriteLine($"                                           seven: {predictedImage.Digit[7]:0.####}");
            Console.WriteLine($"                                           eight: {predictedImage.Digit[8]:0.####}");
            Console.WriteLine($"                                           nine:  {predictedImage.Digit[9]:0.####}");
        }
    }
}
