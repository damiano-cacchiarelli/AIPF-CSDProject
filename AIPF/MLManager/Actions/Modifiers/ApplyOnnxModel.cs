using AIPF.Utilities;
using Microsoft.ML;
using System.Diagnostics;
using System.Linq;

namespace AIPF.MLManager.Actions.Modifiers
{
    public class ApplyOnnxModel<I, O> : IModifier<I, O> where O : class, new()
    {
        public static readonly ActivitySource source = new ActivitySource("Transformer");

        private string modelPath;
        private string[] inputColumnNames;
        private string[] outputColumnNames;

        public string Name
        {
            get
            {
                return modelPath.Split("/").Last();
            }
        }

        public ApplyOnnxModel(string modelPath, string[] inputColumnNames = null, string[] outputColumnNames = null)
        {
            this.modelPath = modelPath;
            this.inputColumnNames = inputColumnNames;
            this.outputColumnNames = outputColumnNames;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            using var activity = source.StartActivity("GetPipeline");
            activity?.AddTag("transformer.name", GetType().SimpleName());
            activity?.AddTag("transformer.algorithm.path", Name);

            inputColumnNames ??= new string[] { };
            outputColumnNames ??= new string[] { };
            return mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, outputColumnNames: outputColumnNames, inputColumnNames: inputColumnNames);
        }
    }
}
