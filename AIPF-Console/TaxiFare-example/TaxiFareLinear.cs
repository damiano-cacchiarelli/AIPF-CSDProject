using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Actions.Modifiers;
using AIPF.MLManager.Actions.Modifiers.Columns;
using AIPF.MLManager.Actions.Modifiers.Date;
using AIPF.MLManager.Actions.Modifiers.Maths;
using AIPF_Console.TaxiFare_example.Model;

namespace AIPF_Console.TaxiFare_example
{
    public class TaxiFareLinear : TaxiFare
    {
        private static TaxiFareLinear instance = new TaxiFareLinear();

        protected TaxiFareLinear() : base("Taxi-Fare-Linear") { }

        public static IExample Start()
        {
            return instance;
        }

        protected override void CreatePipeline()
        {
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new ProgressIndicator<RawStringTaxiFare>($"{Name}Process#1"))
                .Append(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyEvaluableOnnxModel<ProcessedTaxiFare, PredictedFareAmount, RegressionEvaluate>(
                    $"{IExample.Dir}/TaxiFare-example/Data/Onnx/skl_linReg.onnx",
                    (i, o) =>
                    {
                        o.PredictedFareAmount = i.FareAmount[0];
                    }))
                .Build();
        }
    }
}
