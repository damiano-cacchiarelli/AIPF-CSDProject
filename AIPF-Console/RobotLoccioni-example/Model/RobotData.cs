using System;
using System.Collections.Generic;
using System.Text;
using AIPF.MLManager;
using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;

namespace AIPF_Console.RobotLoccioni_example.Model
{
    public class RobotData : IDateAsString, ICopy<RobotData>
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

        public void Copy(ref RobotData b)
        {
            b.DateAsString = DateAsString;
            b.EventType = EventType;
            b.MaxCurrentAxis1 = MaxCurrentAxis1;
            b.MaxCurrentAxis2 = MaxCurrentAxis2;
            b.MaxCurrentAxis3 = MaxCurrentAxis3;
            b.MaxCurrentAxis4 = MaxCurrentAxis4;
            b.MaxCurrentAxis5 = MaxCurrentAxis5;
            b.MaxCurrentAxis6 = MaxCurrentAxis6;
            b.RMSCurrentAxis1 = RMSCurrentAxis1;
            b.RMSCurrentAxis2 = RMSCurrentAxis2;
            b.RMSCurrentAxis3 = RMSCurrentAxis3;
            b.RMSCurrentAxis4 = RMSCurrentAxis4;
            b.RMSCurrentAxis5 = RMSCurrentAxis5;
            b.RMSCurrentAxis6 = RMSCurrentAxis6;
        }
    }
}
