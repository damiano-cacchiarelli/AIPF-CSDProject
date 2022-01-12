using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public interface IFilterAction<I> : IAction where I : class, new()
    {
        public MLContext MLContext { set; }

        bool ApplyFilter(I item);
    }
}
