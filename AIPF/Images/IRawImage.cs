using Microsoft.ML.Data;

namespace AIPF.Images
{
    public interface IRawImage
    {
        [VectorType(32 * 32 * 1)]
        public float[] Elements { get; set; }

        public byte Digit { get; set; }
    }
}