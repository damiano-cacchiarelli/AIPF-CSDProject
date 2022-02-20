namespace AIPF.MLManager.Metrics
{
    public class MetricOptions
    {
        public string Name { get; private set; }
        public double Value { get; private set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double IsBetterIfCloserTo { get; set; }

        public MetricOptions(string name, double value)
        {
            Name = name;
            Value = value;
        }
/*
        public override string ToString()
        {
            string toString = $"{Name}: {Value}";
            toString += IsBetterIfCloserTo != null ? $", is better if closer to { IsBetterIfCloserTo}" : "";
            if (Min != null && Max != null)
            {
                toString += $" (minimum value = {Min}, maximum value = {Max})";
            }
            else
            {
                toString += Min != null ? $" (minimum value = {Min})" : "";
                toString += Max != null ? $" (maximum value = {Max})" : "";
            }
            return toString;
        }
*/
    }
}
