using AIPF.MLManager.Modifiers.Maths;

namespace AIPF_Console.TaxiFare_example.Model
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