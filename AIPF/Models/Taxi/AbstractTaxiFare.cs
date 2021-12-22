using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Data
{
    public abstract class AbstractTaxiFare
    {
        /*
         
         key;fare_amount;pickup_datetime;pickup_longitude;pickup_latitude;dropoff_longitude;dropoff_latitude;passenger_count
         
         */
        [LoadColumn(1)]
        public float FareAmount { get; set; }
        [LoadColumn(3)]
        public float PickupLongitude { get; set; }
        [LoadColumn(4)]
        public float PickupLatitude { get; set; }
        [LoadColumn(5)]
        public float DropoffLongitude { get; set; }
        [LoadColumn(6)]
        public float DropoffLatitude { get; set; }
        [LoadColumn(7)]
        public float PassegerCount { get; set; }



    }
}
