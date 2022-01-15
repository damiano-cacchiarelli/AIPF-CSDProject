using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System.Collections.Generic;

namespace AIPF.MLManager.Actions
{
    public interface IFilterAction<I> : IAction where I : class, new()
    {
        public MLContext MLContext { set; }

        /// <summary>
        /// Represent the filter to be applied to the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if the item satisfy the filter, False otherwise</returns>
        bool ApplyFilter(I item);
    }
}
