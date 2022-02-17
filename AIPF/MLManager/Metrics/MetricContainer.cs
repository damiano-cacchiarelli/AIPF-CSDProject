using System;
using System.Collections.Generic;

namespace AIPF.MLManager.Metrics
{
    public class MetricContainer
    {
        public string Name { get; private set; }
        public List<MetricOptions> Metrics { get; private set; } = new List<MetricOptions>();

        public MetricContainer(string name)
        {
            Name = name;
        }

        public void AddMetric(MetricOptions metric, MetricAddingType writingType = MetricAddingType.New)
        {
            if (ExistsMetric(metric.Name, out MetricOptions existingMetric)) 
            {
                switch (writingType)
                {
                    case MetricAddingType.New:
                        throw new NotImplementedException();
                    case MetricAddingType.Overwrite:
                        throw new NotImplementedException();
                    case MetricAddingType.Skip:
                        throw new NotImplementedException();
                    case MetricAddingType.Min:
                        throw new NotImplementedException();
                    case MetricAddingType.Max:
                        throw new NotImplementedException();
                    case MetricAddingType.Mean:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                Metrics.Add(metric);
            }
        }

        public MetricOptions GetMetric(string name)
        {
            return Metrics.Find(m => m.Name == name);
        }

        public bool ExistsMetric(string name, out MetricOptions existingMetric)
        {
            existingMetric = Metrics.Find(m => m.Name == name);
            return existingMetric != null;
        }

        public override string ToString()
        {
            string line = string.Join("\n\t-- ", Metrics);
            return $"{Name}\n\t-- {line}";
        }
    }

    public enum MetricAddingType
    {
        /// <summary>
        /// if the metric is already presents, save as new
        /// </summary>
        New,

        /// <summary>
        /// if the metric is already presents, it will be overrided
        /// </summary>
        Overwrite,

        /// <summary>
        /// if the metric is already presents, simply don't do nothing
        /// </summary>
        Skip,

        /// <summary>
        /// if the metric is already presents, save the smallest one
        /// </summary>
        Min,

        /// <summary>
        /// if the metric is already presents, save the greatest one
        /// </summary>
        Max,

        /// <summary>
        /// if the metric is already presents, save the mean of the two
        /// </summary>
        Mean
    }
}
