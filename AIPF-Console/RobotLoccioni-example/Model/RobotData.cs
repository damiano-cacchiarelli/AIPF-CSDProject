using System;
using System.Collections.Generic;
using System.Text;
using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;

namespace AIPF_Console.RobotLoccioni_example.Model
{
    public class RobotData : IDateAsString
    {
        [LoadColumn(0)]
        public string DateAsString { get; set; }
        [LoadColumn(1)]
        public float EventType { get; set; }
        [LoadColumn(2)]
        public float MaxCurrentAxis1 { get; set; }
        [LoadColumn(3)]
        public float MaxCurrentAxis2 { get; set; }
        [LoadColumn(4)]
        public float MaxCurrentAxis3 { get; set; }
        [LoadColumn(5)]
        public float MaxCurrentAxis4 { get; set; }
        [LoadColumn(6)]
        public float MaxCurrentAxis5 { get; set; }
        [LoadColumn(7)]
        public float MaxCurrentAxis6 { get; set; }
        [LoadColumn(8)]
        public float RMSCurrentAxis1 { get; set; }
        [LoadColumn(9)]
        public float RMSCurrentAxis2 { get; set; }
        [LoadColumn(10)]
        public float RMSCurrentAxis3 { get; set; }
        [LoadColumn(11)]
        public float RMSCurrentAxis4 { get; set; }
        [LoadColumn(12)]
        public float RMSCurrentAxis5 { get; set; }
        [LoadColumn(13)]
        public float RMSCurrentAxis6 { get; set; }
    }
}
