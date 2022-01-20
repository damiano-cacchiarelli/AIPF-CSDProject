using System.Collections.Generic;
using Microsoft.ML;

namespace AIPF.MLManager
{
    public class MLLoader<I> where I : class, new()
    {
        private readonly MLContext mlContext;

        public MLLoader(MLContext mlContext)
        {
            this.mlContext = mlContext;
        }

        public IDataView LoadFile(string path, char separatorChar = ',', bool hasHeader = true)
        {
            return mlContext.Data.LoadFromTextFile<I>(path, separatorChar: separatorChar, hasHeader: hasHeader);
        }
        
        public IEnumerable<I> GetEnumerable(IDataView dataview)
        {
            return mlContext.Data.CreateEnumerable<I>(dataview, reuseRowObject: true);
        }
    }
}
