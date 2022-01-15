namespace AIPF.MLManager.Metrics
{
    public class MetricOptions
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Min { get; set; }
        public string Max { get; set; }
        public string IsBetterIfCloserTo { get; set; }

        public MetricOptions(string name, string value)
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
