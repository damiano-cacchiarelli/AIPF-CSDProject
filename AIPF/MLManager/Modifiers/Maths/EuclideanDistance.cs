using System;
using System.Collections.Generic;
using System.Text;
using AIPF.Data;
using Microsoft.ML;

namespace AIPF.MLManager.Modifiers.Maths
{
    public class EuclideanDistance<I, O> : IModifier<I, O> where I : class, ICoordinates, new() where O : class, IDistance, new()
    {
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, O>(CaluclateEuclideanDistance, null);
        }

        private void CaluclateEuclideanDistance(I input, O output)
        {
            output.Distance = (float) Math.Sqrt(Math.Pow(input.X1 - input.X2, 2) + Math.Pow(input.Y1 - input.Y2, 2));
        }
    }
}
