using System;
using System.Collections.Generic;
using System.Text;
using AIPF.Data;
using AIPF.MLManager.Modifiers.Date;

namespace AIPF.Models.Taxi 
{
    public class MinutesTaxiFare : AbstractTaxiFare, IDateParser<float>
    {
        float IDateParser<float>.Date { get; set; }
    }
    
}
