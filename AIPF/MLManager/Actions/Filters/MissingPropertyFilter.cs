using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AIPF.MLManager.Actions.Filters
{
    public class MissingPropertyFilter<I> : IFilterAction<I> where I : class, new()
    {
        public MLContext MLContext { set; get; }

        private readonly string[] columnNames;

        private string missingValueFieldName = string.Empty;

        public MissingPropertyFilter(params string[] columnNames)
        {
            this.columnNames = columnNames;
        }

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            trasformedDataView = MLContext.Data.FilterByCustomPredicate<I>(dataView, ApplyFilter);
        }

        public bool ApplyFilter(I item)
        {
            if (columnNames.Length == 0)
            {
                PropertyInfo[] fields = item.GetType().GetProperties();
                foreach (var field in fields)
                {
                    if (field.GetValue(item) == null)
                    {
                        missingValueFieldName = field.Name;
                        return false;
                    }
                }
            }
            else
            {
                foreach (var name in columnNames)
                {
                    PropertyInfo field = item.GetType().GetProperty(name);
                    if (field == null)
                    {
                        throw new MissingFieldException($"The specified column '{name}' does not exist");
                    }

                    if (field.GetValue(item) == null)
                    {
                        missingValueFieldName = field.Name;
                        return false;
                    }
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"Missing value for field {missingValueFieldName}";
        }

        public List<MetricContainer> Evaluate(IDataView dataView, out IDataView transformedDataView)
        {
            Execute(dataView, out transformedDataView);
            return new List<MetricContainer>();
        }
    }
}
