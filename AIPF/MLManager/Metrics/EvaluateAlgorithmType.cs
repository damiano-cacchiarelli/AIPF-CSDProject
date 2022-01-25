namespace AIPF.MLManager.Metrics
{
    public enum EvaluateAlgorithmType
    {
        BINARY,

        /// <summary>
        /// The class must have the attributes for: labelColumnName, scoreColumnName, predictedLabelColumnName.
        /// </summary>
        MULTICLASS,
        REGRESSION,
        CLUSTERING
    }
}
