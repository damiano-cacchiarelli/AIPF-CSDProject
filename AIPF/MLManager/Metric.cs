using System.Collections.Generic;

namespace AIPF.MLManager
{
    public class Metric
    {
        public string Name { get; private set; }
        public Dictionary<string, string> Properties { get; private set; }

        public Metric(string name, Dictionary<string, string> properties)
        {
            Name = name;
            Properties = properties;
        }

        public override string ToString()
        {
            string line = string.Join("\n\t-- ", Properties);
            return $"{Name}\n\t-- {line}";
        }
    }
}
