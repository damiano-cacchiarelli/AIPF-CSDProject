using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AIPF.MLManager.Metrics;
using Microsoft.ML;

namespace AIPF.MLManager.Actions
{
    public class Filter<I> : IFilterAction<I> where I : class, new()
    {
        public MLContext MLContext { get; set; }
        private readonly Expression<Func<I, bool>> filterFunction;

        public Filter(Expression<Func<I, bool>> filterFunction)
        {
            this.filterFunction = filterFunction;
        }

        public virtual void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            trasformedDataView = MLContext.Data.FilterByCustomPredicate<I>(dataView, i => !ApplyFilter(i));
        }

        public bool ApplyFilter(I item)
        {
            return filterFunction.Compile()(item);
        }

        public override string ToString()
        {
            string expBody = filterFunction.Body.ToString();

            var paramName = filterFunction.Parameters[0].Name;
            var paramTypeName = filterFunction.Parameters[0].Type.Name;

            expBody = expBody.Replace(paramName + ".", paramTypeName + ".");
           
            return expBody;
        }

        public List<MetricContainer> Evaluate(IDataView dataView, out IDataView transformedDataView)
        {
            Execute(dataView, out transformedDataView);
            return new List<MetricContainer>();
        }
    }
}
