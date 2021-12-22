using AIPF.MLManager.Modifiers.Date;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.Data
{
    public class DateTimeTaxiFare: AbstractTaxiFare, IDateAsDateTime
    {
        /*
         
         key;fare_amount;pickup_datetime;pickup_longitude;pickup_latitude;dropoff_longitude;dropoff_latitude;passenger_count
         
         */

        public DateTime DateAsDateTime { get; set; }




    }
}
