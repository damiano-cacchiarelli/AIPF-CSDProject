using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace AIPF.MLManager.Modifiers.Date
{
    public class GenericDateParser<I, R, O> : IModifier<I, O> where I : class, IDateAsString, new() where O : class, IDateParser<R>, new()
    {
        private string dateFormat;
        private Func<DateTime, R> function;

        public GenericDateParser(string dateTimeFormat, Func<DateTime, R> function = null)
        {
            this.dateFormat = dateTimeFormat;
            this.function = function;
        }
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, O>(ParsingDate, null);
        }

        private void ParsingDate(I input, O output)
        {
            if (DateTime.TryParseExact(input.DateAsString,
                                        dateFormat,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var parseDate))
            {
                if(function == null)
                {
                    output.Date = output.ToR(parseDate);
                }
                else
                {
                    output.Date = function.Invoke(parseDate);
                }
               
            }
            else
            {
                throw new Exception($"ParseDate exception... {input.DateAsString}");
            }

        }
    }
}
