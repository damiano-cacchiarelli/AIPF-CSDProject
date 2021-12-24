﻿using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Data
{
    public abstract class AbstractTaxiFare : ICoordinates
    {
        /*
         
         key;fare_amount;pickup_datetime;pickup_longitude;pickup_latitude;dropoff_longitude;dropoff_latitude;passenger_count
         
         */
        [LoadColumn(1)]
        public float FareAmount { get; set; }
        [LoadColumn(3)]
        public float X1 { get; set; }
        [LoadColumn(4)]
        public float Y1 { get; set; }
        [LoadColumn(5)]
        public float X2 { get; set; }
        [LoadColumn(6)]
        public float Y2 { get; set; }
        [LoadColumn(7)]
        public float PassengersCount { get; set; }
    }
}
