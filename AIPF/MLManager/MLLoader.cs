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

            IDataView filteredData = mlContext.Data.FilterRowsByColumn(data, nameof(TaxiFareRaw.PassegerCount), lowerBound: 1, upperBound: 10);

            filteredData = mlContext.Data.FilterRowsByMissingValues(filteredData, new[] { 
                nameof(TaxiFareRaw.FareAmount), 
                nameof(TaxiFareRaw.PickupDatetime), 
                nameof(TaxiFareRaw.PickupLongitude), 
                nameof(TaxiFareRaw.PickupLatitude), 
                nameof(TaxiFareRaw.DropoffLongitude), 
                nameof(TaxiFareRaw.DropoffLatitude), 
                nameof(TaxiFareRaw.PassegerCount) 
            });

            return filteredData;

        }
    }
}
