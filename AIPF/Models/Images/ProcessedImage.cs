using Microsoft.ML.Data;
using AIPF.MLManager;

namespace AIPF.Models.Images
{
    public class ProcessedImage : ICopy<ProcessedImage>, IProcessedImage
    {
        [VectorType(64)]
        public float[] Pixels { get; set; }

        public byte Digit { get; set; }

        public void Copy(ref ProcessedImage b)
        {
            b.Pixels = Pixels;
            b.Digit = Digit;
        }

        public override string ToString()
        {
            return $"Pixels: {Pixels.Length}, Digit: {Digit}";
        }
    }
}
