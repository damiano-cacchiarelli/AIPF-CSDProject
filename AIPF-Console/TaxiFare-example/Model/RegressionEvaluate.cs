using AIPF.MLManager.Metrics;

namespace AIPF_Console.TaxiFare_example.Model
{
    [EvaluateAlgorithm(EvaluateAlgorithmType.REGRESSION, "labelColumnName", "FareAmount")]
    public class RegressionEvaluate
    {
        [EvaluateColumn("scoreColumnName")]
        public float PredictedFareAmount { get; set; }
    }
}
