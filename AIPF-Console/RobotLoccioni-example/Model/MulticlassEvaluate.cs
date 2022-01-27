using AIPF.MLManager.Metrics;
using Microsoft.ML.Data;

namespace AIPF_Console.RobotLoccioni_example.Model
{
    /*
    public class MulticlassEvaluate : GenericEvaluateOutput
    {
        [EvaluateColumn("scoreColumnName")]
        [VectorType(10)]
        public float[] ProbabilityEventType { get; set; }

        [EvaluateColumn("predictedLabelColumnName")]
        public float PredictedEventType { get; set; }

        public MulticlassEvaluate() : base(EvaluateAlgorithmType.MULTICLASS) { }
    }
*/

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
