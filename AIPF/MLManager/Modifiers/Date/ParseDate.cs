using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIPF.MLManager.Modifiers.Date
{
    public class ParseDate<I, O> : IModifier<I, O> where I : class, IDateAsString, new() where O : class, IDateAsDateTime, new()
    {

        private string dateFormat;

        public ParseDate(string dateTimeFormat)
        {
            this.dateFormat = dateTimeFormat;
        }
        public IEstimator<ITransformer> GetPipeline(MLContext mlContext)
        {
            return mlContext.Transforms.CustomMapping<I, O>(ParsingData, null);
        }

        private void ParsingData(I input, O output)
        {
            /*
            output.Col1 = input.Col1;
            input.Copy(ref output);

            if (DateTime.TryParseExact(input.DateOfJourneyStr,
                                        DATETIME_FORMAT,
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var result))
            */
        }
    }
}
