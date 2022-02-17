using AIPF.MLManager.Metrics;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (columnNames.Length == 0)
                columnNames = GetAllProperties();
            this.columnNames = columnNames;
        }

        public void Execute(IDataView dataView, out IDataView trasformedDataView)
        {
            trasformedDataView = MLContext.Data.FilterByCustomPredicate<I>(dataView, i => !ApplyFilter(i));
        }

       /*
        public bool ApplyFilter(I item)
        {
            var dataView = MLContext.Data.LoadFromEnumerable(new[] { item });
            Execute(dataView, out IDataView trasformedDataView);
            return trasformedDataView.Preview().RowView.Length == 1;
        }
       */
        
        public bool ApplyFilter(I item)
        {
            foreach (var name in columnNames)
            {
                PropertyInfo field = item.GetType().GetProperty(name);
                if (field == null)
                {
                    throw new MissingFieldException($"The specified column '{name}' does not exist");
                }

                var i = field.GetValue(item);

                if (i == null || 
                    MissingType(i, string.Empty) ||
                    MissingType(i, float.NaN) ||
                    MissingType(i, double.NaN))
                {
                    missingValueFieldName = field.Name;
                    return false;
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

        private string[] GetAllProperties()
        {
            return typeof(I).GetProperties()
                //.Where(field => !field.PropertyType.IsAssignableFrom(typeof(string)))
                .Select(field => field.Name)
                .ToArray();
        }

        private bool MissingType<T>(object i, T invalid)
        {
            return i.GetType().IsAssignableFrom(typeof(T)) && ((T)i).Equals(invalid);
        }
    }
}
