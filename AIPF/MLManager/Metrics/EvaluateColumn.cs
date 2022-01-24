using System;
using System.Collections.Generic;
using System.Linq;

namespace AIPF.MLManager.Metrics
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EvaluateColumn : Attribute
    {
        private string columnName;

        public EvaluateColumn( string columnName)
        {
            this.columnName = columnName;
        }

        public string GetColumnName() { return columnName; }

        public static Dictionary<string, string> GetDictionary(Type type)
        {
            Dictionary<string, string> columns = new Dictionary<string, string>();
            var props = type.GetProperties();
            foreach (var item in props)
            {
                foreach (var attr in item.GetCustomAttributes(true))
                {
                    if (attr is EvaluateColumn)
                    {
                        var prop = attr as EvaluateColumn;
                        string propName = prop.GetColumnName();
                        string auth = item.Name;
                        columns.Add(propName, auth);
                    }
                }
            }
            return columns;
        }

    }
}
