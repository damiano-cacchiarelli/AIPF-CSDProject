using Microsoft.ML.Data;

namespace AIPF_Console.MNIST_example.Model
{
    public interface IOutputImage : IProcessedImage
    {
        [ColumnName("Score")]
        public float[] Digit { get; set; }
    }
}