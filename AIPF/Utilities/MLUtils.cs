using Microsoft.ML;
using System.Linq;

namespace AIPF.Utilities
{
    public static class MLUtils
    {
        public static long GetDataViewLength<I>(MLContext mlContext, IDataView dataView) where I : class, new()
        {
            long count = dataView.GetRowCount() ?? -1;
            if (count == -1)
                count = mlContext.Data.CreateEnumerable<I>(dataView, reuseRowObject: true).Count();
            return count;
        }
    }
}
