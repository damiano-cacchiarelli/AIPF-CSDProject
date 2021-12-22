using Microsoft.ML.Data;

namespace AIPF.Images
{
    public interface IOutputImage : IProcessedImage
    {
        [ColumnName("Score")]
        public float[] Digit { get; set; }
    }
}