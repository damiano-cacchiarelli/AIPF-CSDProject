using Microsoft.ML.Data;

namespace AIPF_Console.MNIST_example.Model
{
    public interface IProcessedImage
    {
        [VectorType(64)]
        public float[] Pixels { get; set; }
    }
}