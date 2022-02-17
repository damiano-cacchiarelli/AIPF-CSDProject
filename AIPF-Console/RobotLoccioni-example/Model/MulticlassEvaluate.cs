using AIPF.MLManager.Metrics;
using Microsoft.ML.Data;

namespace AIPF_Console.RobotLoccioni_example.Model
{

    [EvaluateAlgorithm(EvaluateAlgorithmType.MULTICLASS, "labelColumnName", "EventType")]
    public class MulticlassEvaluate
    {
        [EvaluateColumn("scoreColumnName")]
        [VectorType(10)]
        public float[] ProbabilityEventType { get; set; }

        [EvaluateColumn("predictedLabelColumnName")]
        public float PredictedEventType { get; set; }
    }
}
