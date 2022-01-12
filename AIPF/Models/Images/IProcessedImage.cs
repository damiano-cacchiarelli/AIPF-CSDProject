using Microsoft.ML.Data;

namespace AIPF.Models.Images
{
    public interface IProcessedImage
    {
        [VectorType(64)]
        public float[] Pixels { get; set; }
    }
}