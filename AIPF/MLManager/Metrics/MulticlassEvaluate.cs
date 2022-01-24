using Microsoft.ML.Data;

namespace AIPF.MLManager.Metrics
{
    public class MulticlassEvaluate
    {
        //[EvaluateColumn("labelColumnName")]
        //public float EventType { get; set; }

        [EvaluateColumn("scoreColumnName")]
        [VectorType(9)]
        public float[] ProbabilityEventType { get; set; }

        [EvaluateColumn("predictedLabelColumnName")]
        public float PredictedEventType { get; set; }
    }
}
