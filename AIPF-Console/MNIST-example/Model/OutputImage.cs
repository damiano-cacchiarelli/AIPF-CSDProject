using Microsoft.ML.Data;

namespace AIPF_Console.MNIST_example.Model
{
    public class OutputImage : IOutputImage
    {
        [VectorType(64)]
        public float[] Pixels { get; set; }

        [ColumnName("Score")]
        public float[] Digit { get; set; }
    }
}