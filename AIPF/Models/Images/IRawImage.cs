using Microsoft.ML.Data;

namespace AIPF.Images
{
    public interface IRawImage<T>
    {
        public static int Width { get; }
        
        public static int Height { get; }

        public T Elements { get; set; }

        public byte Digit { get; set; }
    }
}