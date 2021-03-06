using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;

namespace AIPF_Console.RobotLoccioni_example.Model
{
    public class OutputMeasure
    {
        [ColumnName("output_label")]
        public long[] EventType { get; set; }


        /*
         * Before: 
         *  [ColumnName("output_probability")]
         *  public IEnumerable<IDictionary<long, float>> Probability { get; set; }
         *  
         * Problem:
         *  Could not determine an IDataView type and registered custom types for member Probability(Parameter 'rawType') 
         *  
         * Solution: 
         *  [ColumnName("output_probability")]
         *  [OnnxSequenceType(typeof(IDictionary<long, float>))]
         *  public IEnumerable<IDictionary<long, float>> Probability { get; set; }
         * 
         */
        [ColumnName("output_probability")]
        [OnnxSequenceType(typeof(IDictionary<long, float>))]
        public IEnumerable<IDictionary<long, float>> Probability { get; set; }

        public string EventTypeName()
        {
            switch (EventType[0])
            {
                case 0:
                    return "Error in saving";
                case long n when (n >= 1 && n <= 4):
                    return "Productive cicle";
                case long n when (n == 6 || n == 8 || n == 9):
                    return "Maintenance";
                case 7:
                    return "Diagnostics";
                case 5:
                    return "Ordinary maintenance";
                default:
                    return "Unknown";
            }
        }

        public float[] GetProbability() 
        {
            var dic = Probability.ToArray()[0];
            //dic.Add(0, 0);
            var toReturn = new List<float>() { 0 };
            toReturn.AddRange(dic.Values);
            return toReturn.ToArray();
        }
    }
}
