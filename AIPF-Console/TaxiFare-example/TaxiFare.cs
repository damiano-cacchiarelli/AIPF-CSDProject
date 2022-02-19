using AIPF.MLManager;
using AIPF.MLManager.Metrics;
using AIPF_Console.TaxiFare_example.Model;
using AIPF_Console.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIPF_Console.TaxiFare_example
{
    public abstract class TaxiFare : IExample
    {
        protected MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager;

        private string name;
        public string Name { get => name; private set => name = value; }

        protected TaxiFare(string name)
        {
            Name = name;
            mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>(name);
        }

        protected abstract void CreatePipeline();

        public async Task Train(int? numberOfIterations, int? numberOfElementsForTrain)
        {
            AnsiConsole.Write(new Rule("[yellow]Training[/]").RuleStyle("grey").LeftAligned());

            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = new object[0] };
                RestService.Post<string>("train", fitBody);
            }
            else if (!mlManager.Trained)
            {
                CreatePipeline();

                var data = new RawStringTaxiFare[] { };
                var fitTask = mlManager.Fit(data);
                await ConsoleHelper.Loading("Fitting model", $"{Name}Process#1");
                await fitTask;
            }

            AnsiConsole.WriteLine("Train complete");
        }

        public async Task Predict(PredictionMode predictionMode = PredictionMode.USER_VALUE, int error = 0)
        {

            AnsiConsole.Write(new Rule("[yellow]Predicting[/]").RuleStyle("grey").LeftAligned());

            var pickupDatetime = "2011-08-18 00:35:00";
            var pickup_longitude = -73.982738f;
            var pickup_latitude = 40.76127f;
            var dropoff_longitude = -73.991242f;
            var dropoff_latitude = 40.750562f;
            float passenger_count = 2;

            if (predictionMode == PredictionMode.USER_VALUE)
            {
                pickupDatetime = AnsiConsole.Ask<string>("Insert the pickup datetime (must be of the format YYYY-MM-DD hh:mm:ss) ", "2011-08-18 00:35:00");
                pickup_longitude = AnsiConsole.Ask<float>("Insert the pickup longitude ", -73.982738f);
                pickup_latitude = AnsiConsole.Ask<float>("Insert the pickup latitude ", 40.76127f);
                dropoff_longitude = AnsiConsole.Ask<float>("Insert the dropoff longitude ", -73.991242f);
                dropoff_latitude = AnsiConsole.Ask<float>("Insert the dropoff latitude ", 40.750562f);
                passenger_count = AnsiConsole.Ask<float>("Insert the passenger count ", 2);
            }
            else if (predictionMode == PredictionMode.RANDOM_VALUE)
            {
                error = Math.Clamp(error, 0, 100);

                var random = new Random();
                pickup_longitude = (float)random.NextDouble() * -73;
                dropoff_longitude = pickup_longitude + (float)random.NextDouble()*0.3f;
                pickup_latitude = (float)random.NextDouble() * 40;
                dropoff_latitude = pickup_latitude + (float)random.NextDouble()*0.3f;
                passenger_count = random.Next(1, 9);

                if (random.Next(0, 100) < error) dropoff_latitude += 0.1f;
                if (random.Next(0, 100) < error) passenger_count += 5;
            }

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
                predictedValue = await mlManager.Predict(toPredict);
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

        public async Task Metrics(int? numberOfElementsForEvaluate)
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
                var rawDataList = mlManager.Loader.GetEnumerable(data);
                IEnumerable<RawStringTaxiFare> rawDataListForEvaluate = rawDataList.OrderBy(x => IExample.random.Next());
                if(numberOfElementsForEvaluate != null) 
                    rawDataListForEvaluate = rawDataListForEvaluate.Take((int)numberOfElementsForEvaluate);
                var taskMetrics = mlManager.EvaluateAll(rawDataListForEvaluate);
                await ConsoleHelper.Loading("Evaluating model", $"{Name}Process#1");
                metrics = await taskMetrics;
            }

            ConsoleHelper.PrintMetrics(metrics);
        }
    }
}
