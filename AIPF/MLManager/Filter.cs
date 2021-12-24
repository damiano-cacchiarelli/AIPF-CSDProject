using System;
using Microsoft.ML;

namespace AIPF.MLManager
{
    public class Filter<I> : IFilterAction<I> where I : class, new()
    {
        private readonly MLContext mlContext;
        private readonly Func<I, bool> filterFunction;

        public Filter(MLContext mlContext, Func<I, bool> filterFunction)
        {
            this.mlContext = mlContext;
            this.filterFunction = filterFunction;
        }

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            trasformedDataView = mlContext.Data.FilterByCustomPredicate<I>(dataView, ApplyFilter);
        }

        public bool ApplyFilter(I item)
        {
            return filterFunction.Invoke(item);
        }
    }
}
