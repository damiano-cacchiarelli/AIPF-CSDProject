using System.Collections.Generic;
using Microsoft.ML.Data;
using AIPF.MLManager;
using System;

namespace AIPF.Images
{
    public class RawImage : IRawImage, ICopy<RawImage>
    {
        [VectorType(32 * 32 * 3)]
        public float[] Elements { get; set; }

        public byte Digit { get; set; }

        public RawImage() { }

        public RawImage(List<string> charList, string digitLine)
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
                    float color = ((float)char.GetNumericValue(c) * 255);
                    list.Add(color); // r = 0 / 255
                    list.Add(color); // g = 0 / 255
                    list.Add(color); // b = 0 / 255
                }
            });
            Elements = list.ToArray();

            
        }

        private void ParseToByte(string digitLine)
        {
            Digit = (byte)char.GetNumericValue(digitLine.ToCharArray()[1]);
        }

        public void Copy(ref RawImage b)
        {
            b.Digit = Digit;
            b.Elements = Elements;
        }
    }
}
