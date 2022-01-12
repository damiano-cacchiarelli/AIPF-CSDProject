using System.Collections.Generic;
using Microsoft.ML.Data;
using AIPF.MLManager;

namespace AIPF.Models.Images
{
    public class VectorRawImage : IRawImage<float[]>, ICopy<VectorRawImage>
    {
        public static int Width => 32;
        public static int Height => 32;

        [VectorType(32 * 32 * 1)]
        public float[] Elements { get; set; }

        public byte Digit { get; set; }

        public VectorRawImage() { }

        public VectorRawImage(List<string> charList, string digitLine)
        {
            ParseToFloatVector(charList);
            ParseToByte(digitLine);
        }

        private void ParseToFloatVector(List<string> charList)
        {
            List<float> list = new List<float>();
            charList.ForEach(row =>
            {
                foreach (char c in row)
                {
                    list.Add((float)char.GetNumericValue(c));
                }
            });
            Elements = list.ToArray();
        }

        private void ParseToByte(string digitLine)
        {
            Digit = (byte)char.GetNumericValue(digitLine.ToCharArray()[1]);
        }

        public void Copy(ref VectorRawImage b)
        {
            b.Digit = Digit;
            b.Elements = Elements;
        }
    }
}
