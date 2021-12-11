using Microsoft.ML;
using AIPF.Images;

namespace AIPF.MLManager.Modifiers
{
    public abstract class AbstractImageResizer<I, T, O> : IModifier<I, O> where I : class, IRawImage<T>, new() where O : class, IProcessedImage, new()
    {
        public int ResizedWidth { get; private set; }
        public int ResizedHeight { get; private set; }
        public bool ApplyGrayScale { get; private set; }

        public AbstractImageResizer(int resizedWidth = 8, int resizedHeight = 8, bool applyGrayScale = false)
        {
            ResizedWidth = resizedWidth;
            ResizedHeight = resizedHeight;
            ApplyGrayScale = applyGrayScale;
        }

        public abstract IEstimator<ITransformer> GetPipeline(MLContext mlContext);
    }
}