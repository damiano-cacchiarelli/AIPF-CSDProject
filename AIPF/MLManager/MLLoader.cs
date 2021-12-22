using AIPF.Data;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.MLManager
{
    public class MLLoader<I>
    {
        private readonly MLContext mlContext;
        public MLLoader(MLContext mlContext)
        {
            this.mlContext = mlContext;
        }

        public IDataView Load(string path, char separatorChar = ';')
        {

            var data = this.mlContext.Data.LoadFromTextFile<I>(path, separatorChar: separatorChar, hasHeader: true);

            IDataView filteredData = mlContext.Data.FilterRowsByColumn(data, nameof(RawStringTaxiFare.PassengersCount), lowerBound: 1, upperBound: 10);

            filteredData = mlContext.Data.FilterRowsByMissingValues(filteredData, new[] { 
                nameof(RawStringTaxiFare.FareAmount), 
               // nameof(RawStringTaxiFare.DateAsString), 
                nameof(RawStringTaxiFare.X1), 
                nameof(RawStringTaxiFare.X2), 
                nameof(RawStringTaxiFare.Y1), 
                nameof(RawStringTaxiFare.Y2), 
                nameof(RawStringTaxiFare.PassengersCount) 
            });

            return filteredData;

        }
    }
}
