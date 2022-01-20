using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.Models.Taxi;
using AIPF_Console.Utils;
using Microsoft.ML;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace AIPF_Console.TaxiFare_example
{
    public class TaxiFare : IExample
    {

        private MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();

        public string Name => "Taxi-Fare";

        protected TaxiFare()
        {

        }

        public static IExample Start()
        {
            return new TaxiFare();

        }

        public void Train()
        {
            AnsiConsole.Write(new Rule("[yellow]Training[/]").RuleStyle("grey").LeftAligned());

            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = new object[0] };
                RestService.Post<string>("train", fitBody);
            }
            else
            {

                mlManager.CreatePipeline()
                    .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                    .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                    .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                    .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                    .Build()
                    .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                    .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                    .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{IExample.Dir}/TaxiFare-example/Data/Onnx/skl_pca.onnx"))
                    .Append(new DeleteColumn<object>("input"))
                    .Append(new RenameColumn2<object>("variable", "input"))
                    .Append(new DeleteColumn<object>("variable"))
                    .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{IExample.Dir}/TaxiFare-example/Data/Onnx/skl_pca_linReg.onnx"))
                    .Build();

                var data = new RawStringTaxiFare[] { };
                mlManager.Fit(data, out var dataView);
            }

            ConsoleHelper.FitLoader();

            AnsiConsole.WriteLine("Train complete");
        }

        public void Predict()
        {

            AnsiConsole.Write(new Rule("[yellow]Predicting[/]").RuleStyle("grey").LeftAligned());

            var pickupDatetime = AnsiConsole.Ask<string>("Insert the pickup datetime (must be of the format YYYY-MM-DD hh:mm:ss) ", "2011-08-18 00:35:00");
            var pickup_longitude = AnsiConsole.Ask<float>("Insert the pickup longitude ", -73.982738f);
            var pickup_latitude = AnsiConsole.Ask<float>("Insert the pickup latitude ", 40.76127f);
            var dropoff_longitude = AnsiConsole.Ask<float>("Insert the dropoff longitude ", -73.991242f);
            var dropoff_latitude = AnsiConsole.Ask<float>("Insert the dropoff latitude ", 40.750562f);
            var passenger_count = AnsiConsole.Ask<float>("Insert the passenger count ", 2);

            var toPredict = new RawStringTaxiFare()
            {
                DateAsString = pickupDatetime + " UTC",
                X1 = pickup_longitude,
                Y1 = pickup_latitude,
                X2 = dropoff_longitude,
                Y2 = dropoff_latitude,
                PassengersCount = passenger_count,
            };

            var table = new Table().Centered();
            table.AddColumn("Pickup datetime");
            table.AddColumn("Pickup longitude");
            table.AddColumn("Pickup latitude");
            table.AddColumn("Dropoff longitude");
            table.AddColumn("Dropoff latitude");
            table.AddColumn("Passenger count");
            table.AddColumn("[red]Fare amount[/]");

            PredictedFareAmount predictedValue;
            if (Program.REST)
            {
                predictedValue = RestService.Put<PredictedFareAmount>($"predict/{Name}", toPredict).Result;
            }
            else
            {
                predictedValue = mlManager.Predict(toPredict);
            }



            var values = new string[]{
                pickupDatetime,
                pickup_longitude.ToString(),
                pickup_latitude.ToString(),
                dropoff_longitude.ToString(),
                dropoff_latitude.ToString(),
                passenger_count.ToString(),
                $"[red]{predictedValue.FareAmount[0]}[/]"
            };

            table.AddRow(values);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
        }

        public void Metrics()
        {
            List<MetricContainer> metrics;
            var data = mlManager.Loader.LoadFile($"{IExample.Dir}/TaxiFare-example/Data/train_mini.csv");
            if (Program.REST)
            {
                var rawDataList = mlManager.Loader.GetEnumerable(data).Take(50);
                dynamic fitBody = new { ModelName = Name, Data = rawDataList };
                metrics = RestService.Put<List<MetricContainer>>("metrics", fitBody).Result;
            }
            else
            {
                metrics = mlManager.EvaluateAll(data);
            }

            ConsoleHelper.PrintMetrics(metrics);
        }
    }
}
