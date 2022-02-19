using AIPF.MLManager.Actions.Modifiers;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.MNIST_example.Modifiers;

namespace AIPF_Console.MNIST_example
{
    public class MnistCustom : Mnist
    {
        private static MnistCustom instance = new MnistCustom();

        protected MnistCustom() : base("MNIST-Custom") { }

        protected override void CreatePipeline(int numberOfIterations)
        {
            mlManager.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>($"{Name}Process#1"))
                .Append(new CustomImageResizer())
                .Append(new SdcaMaximumEntropy(numberOfIterations))
                .Build();
        }

        public static IExample Start()
        {
            return instance;
        }
    }
}
