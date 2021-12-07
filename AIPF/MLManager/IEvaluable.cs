using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager
{
    public interface IEvaluable
    {
        Metric Evaluate(MLContext mlContext, IDataView data);
    }
}