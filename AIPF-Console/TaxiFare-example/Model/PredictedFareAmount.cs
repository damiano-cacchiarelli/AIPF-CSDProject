using Microsoft.ML.Data;

namespace AIPF_Console.TaxiFare_example.Model
{
    public class PredictedFareAmount: ProcessedTaxiFare
    {
        [ColumnName("variable")]
        [VectorType(1)]
        public float[] FareAmount { get; set; }
    }
}
