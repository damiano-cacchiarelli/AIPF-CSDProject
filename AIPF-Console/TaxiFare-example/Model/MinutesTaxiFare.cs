using AIPF.MLManager;
using AIPF.MLManager.Actions.Modifiers.Date;

namespace AIPF_Console.TaxiFare_example.Model
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
