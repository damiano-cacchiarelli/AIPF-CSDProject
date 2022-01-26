using AIPF.MLManager;
using AIPF.MLManager.Modifiers.Date;
using AIPF.Models.Taxi;
using Microsoft.ML.Data;

namespace AIPF.Models.Taxi
{
    public class RawStringTaxiFare : AbstractTaxiFare, IDateAsString, ICopy<MinutesTaxiFare>, ICopy<RawStringTaxiFare>
    {
        /*
         
         key;fare_amount;pickup_datetime;pickup_longitude;pickup_latitude;dropoff_longitude;dropoff_latitude;passenger_count
         
         */

        [LoadColumn(2)]
        public string DateAsString { get; set; }

        public void Copy(ref MinutesTaxiFare b)
        {
            b.FareAmount = FareAmount;
            b.X1 = X1;
            b.X2 = X2;
            b.Y1 = Y1;
            b.Y2 = Y2;
            b.PassengersCount = PassengersCount;
        }

        public void Copy(ref RawStringTaxiFare b)
        {
            b.DateAsString = DateAsString;
            b.FareAmount = FareAmount;
            b.PassengersCount = PassengersCount;
            b.X1 = X1;
            b.X2 = X2;
            b.Y1 = Y1;
            b.Y2 = Y2;
        }
    }
}
