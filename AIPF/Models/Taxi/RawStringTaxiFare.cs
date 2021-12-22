using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Data
{
    public class RawStringTaxiFare : AbstractTaxiFare, IDateAsString
    {
        /*
         
         key;fare_amount;pickup_datetime;pickup_longitude;pickup_latitude;dropoff_longitude;dropoff_latitude;passenger_count
         
         */

        [LoadColumn(2)]
        public string DateAsString { get; set; }



    }
}
