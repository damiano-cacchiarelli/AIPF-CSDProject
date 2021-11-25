using Microsoft.ML.Data;

namespace AIPF.Images
{
    public interface IProcessedImage
    {
        [VectorType(64)]
        public float[] Pixels { get; set; }
    }
}