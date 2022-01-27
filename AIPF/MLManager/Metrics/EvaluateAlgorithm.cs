using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIPF.MLManager.Metrics
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EvaluateAlgorithm : Attribute
    {
        private EvaluateAlgorithmType algorithmType;
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public EvaluateAlgorithm(EvaluateAlgorithmType algorithmType, params string[] labels)
        {
            this.algorithmType = algorithmType;
            if (labels.Length % 2 != 0) 
                throw new Exception("The number of labels must be odd");
            for (int i = 0; i < labels.Length; i+=2)
            {
                dictionary.Add(labels[i], labels[i+1]);
            }
        }

        public EvaluateAlgorithmType GetAlgorithmType()
        {
            return algorithmType;
        }

        public Dictionary<string, string> GetDictionary()
        {
            return dictionary;
        }
    }
}
