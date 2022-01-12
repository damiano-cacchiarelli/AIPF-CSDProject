using Microsoft.ML.Data;

namespace AIPF.Models.Taxi
{
    public class PredictedFareAmount: ProcessedTaxiFare
    {
        [ColumnName("variable")]
        [VectorType(1)]
        public float[] FareAmount { get; set; }
    }
}
