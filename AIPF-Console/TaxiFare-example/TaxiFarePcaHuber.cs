using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Actions.Modifiers;
using AIPF.MLManager.Actions.Modifiers.Columns;
using AIPF.MLManager.Actions.Modifiers.Date;
using AIPF.MLManager.Actions.Modifiers.Maths;
using AIPF_Console.TaxiFare_example.Model;

namespace AIPF_Console.TaxiFare_example
{
    public class TaxiFarePcaHuber : TaxiFare
    {
        private static TaxiFarePcaHuber instance = new TaxiFarePcaHuber();

        protected TaxiFarePcaHuber() : base("Taxi-Fare-Pca-Huber") { }

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
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{IExample.Dir}/TaxiFare-example/Data/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyEvaluableOnnxModel<object, PredictedFareAmount, RegressionEvaluate>(
                    $"{IExample.Dir}/TaxiFare-example/Data/Onnx/skl_pca_hubReg.onnx",
                    (i, o) =>
                    {
                        o.PredictedFareAmount = i.FareAmount[0];
                    }))
                .Build();
        }
    }
}
