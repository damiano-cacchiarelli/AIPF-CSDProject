using System.Collections.Generic;
using Microsoft.ML.Data;
using AIPF.MLManager;
using System.Drawing;
using Microsoft.ML.Transforms.Image;

namespace AIPF.Models.Images
{
    public class BitmapRawImage : IRawImage<Bitmap>, ICopy<BitmapRawImage>
    {
        public static int Width => 32;
        public static int Height => 32;

        [ImageType(32, 32)]
        public Bitmap Elements { get; set; }

        public byte Digit { get; set; }

        public BitmapRawImage() { }

        public BitmapRawImage(List<string> charList, string digitLine)
        {
            ParseToBitmap(charList);
            ParseToByte(digitLine);
        }

        private void ParseToBitmap(List<string> charList)
        {
            Elements = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            int positionX = 0;
            int positionY = 0;
            charList.ForEach(row =>
            {
                foreach (char c in row)
                {
                    int color = ((int)char.GetNumericValue(c) * 255);
                    Elements.SetPixel(positionX, positionY, Color.FromArgb(color, color, color));
                    positionX++;
                }
                positionY++;
                positionX = 0;
            });
        }

        private void ParseToByte(string digitLine)
        {
            Digit = (byte)char.GetNumericValue(digitLine.ToCharArray()[1]);
        }

        public void Copy(ref BitmapRawImage b)
        {
            b.Digit = Digit;
            b.Elements = Elements;
        }
    }
}
