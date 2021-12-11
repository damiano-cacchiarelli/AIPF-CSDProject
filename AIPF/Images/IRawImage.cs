using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Image;
using System.Drawing;

namespace AIPF.Images
{
    public interface IRawImage
    {
        [ImageType(32, 32)]
        public Bitmap Elements { get; set; }

        public byte Digit { get; set; }
    }
}