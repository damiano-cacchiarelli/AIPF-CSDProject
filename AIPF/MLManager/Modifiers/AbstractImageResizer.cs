using Microsoft.ML;
using AIPF.Images;

namespace AIPF.MLManager.Modifiers
{
    public abstract class AbstractImageResizer : IModifier<RawImage, ProcessedImage>
    {
        public int OriginalWidth { get; private set; }
        public int OriginalHeight { get; private set; }
        public int ResizedWidth { get; private set; }
        public int ResizedHeight { get; private set; }
        public bool ApplyGrayScale { get; private set; }

        public AbstractImageResizer(int originalWidth = 32, int originalHeight = 32, int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        {
            OriginalWidth = originalWidth;
            OriginalHeight = originalHeight;
            ResizedWidth = resizedWidth;
            ResizedHeight = resizedHeight;
            ApplyGrayScale = applyGrayScale;
        }

        public abstract IEstimator<ITransformer> GetPipeline(MLContext mlContext);
    }
}