using Microsoft.ML;

namespace AIPF.MLManager.Actions.Modifiers
{
    public class ApplyOnnxModel<I, O> : IModifier<I, O> where O : class, new()
    {
        private string modelPath;
        private string[] inputColumnNames;
        private string[] outputColumnNames;


        public ApplyOnnxModel(string modelPath, string[] inputColumnNames = null, string[] outputColumnNames = null)
        {
            this.modelPath = modelPath;
            this.inputColumnNames = inputColumnNames;
            this.outputColumnNames = outputColumnNames;
        }

        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            inputColumnNames ??= new string[] { };
            outputColumnNames ??= new string[] { };
            return mlContext.Transforms.ApplyOnnxModel(modelFile: modelPath, outputColumnNames: outputColumnNames, inputColumnNames: inputColumnNames);
        }
    }
}
