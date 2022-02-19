using AIPF.MLManager.Actions.Modifiers;
using AIPF_Console.MNIST_example.Model;
using AIPF_Console.MNIST_example.Modifiers;

namespace AIPF_Console.MNIST_example
{
    public class MnistDefault : Mnist
    {
        private static MnistDefault instance = new MnistDefault();

        protected MnistDefault() : base("MNIST-Default") { }

        protected override void CreatePipeline(int numberOfIterations)
        {
            mlManager.CreatePipeline()
                .AddTransformer(new ProgressIndicator<VectorRawImage>($"{Name}Process#1"))
                .Append(new VectorImageResizer())
                .Append(new SdcaMaximumEntropy(numberOfIterations))
                .Build();
        }

        public static IExample Start()
        {
            return instance;
        }
    }
}
