using AIPF.MLManager;
using AIPF.MLManager.Modifiers.Date;

namespace AIPF.Models.Taxi 
{
    public class MinutesTaxiFare : AbstractTaxiFare, IDateParser<float>, ICopy<ProcessedTaxiFare>
    {
        public float Date { get; set; }

        public void SetDate(float date)
        {
            Date = date;
        }

        public void Copy(ref ProcessedTaxiFare b)
        {
            b.Date = Date;
            b.PassengersCount = PassengersCount;
        }
    }
    
}
