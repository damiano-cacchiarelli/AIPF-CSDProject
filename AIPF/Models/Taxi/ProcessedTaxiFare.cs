using AIPF.MLManager.Modifiers.Maths;
using Microsoft.ML.Data;

namespace AIPF.Data
{
    public class ProcessedTaxiFare : IDistance
    {
        //[ColumnName("passenger_count")]
        public float PassengersCount { get; set; }
        //[ColumnName("distance")]
        public float Distance { get; set; }
        //[ColumnName("start_time")]
        public float Date { get; set; }
    }
}