using Microsoft.ML;

namespace AIPF.MLManager.Actions
{
    interface ITransformerAction<I, O> : IAction where I : class, new() where O : class, new()
    {
        public ITransformer Model { get; }

        O Predict(I toPredict);
    }
}
