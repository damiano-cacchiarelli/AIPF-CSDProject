using System;
using Microsoft.ML;

namespace AIPF.MLManager.Actions.Modifiers.Maths
{
    public class EuclideanDistance<I, O> : IModifier<I, O> where I : class, ICoordinates, ICopy<O>, new() where O : class, IDistance, new()
    {
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, O>(CaluclateEuclideanDistance, null);
        }

        private void CaluclateEuclideanDistance(I input, O output)
        {
            input.Copy(ref output);
            output.Distance = (float) Math.Sqrt(Math.Pow(input.X1 - input.X2, 2) + Math.Pow(input.Y1 - input.Y2, 2));
        }
    }
}
