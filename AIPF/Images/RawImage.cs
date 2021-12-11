using System.Collections.Generic;
using Microsoft.ML.Data;
using AIPF.MLManager;
using System;
using System.Drawing;
using Microsoft.ML.Transforms.Image;

namespace AIPF.Images
{
    public class RawImage : IRawImage, ICopy<RawImage>
    {
        [ImageType(32,32)]
        public Bitmap Elements { get; set; }

        public byte Digit { get; set; }

        public RawImage() { }

        public RawImage(List<string> charList, string digitLine)
        {
            ParseToFloatVector(charList);
            ParseToByte(digitLine);
        }

        private void ParseToFloatVector(List<string> charList)
        {
            Bitmap list = new Bitmap(32,32,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int x = 0;
            int y = 0;
            charList.ForEach(row =>
            {
                foreach (char c in row)
                {
   
                    int color = ((int)char.GetNumericValue(c) * 255);
                    list.SetPixel(x,y,Color.FromArgb(color, color, color)); // r = 0 / 255
                    //list.Add(color); // g = 0 / 255
                    //list.Add(color); // b = 0 / 255
                    x++;
                }
                y++;
                x = 0;
            });
            Elements = list;

            
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
