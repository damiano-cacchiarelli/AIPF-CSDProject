using System;
using System.Collections.Generic;
using System.Text;
using Spectre.Console;
using System.IO;
using AIPF.MLManager;
using AIPF.Models.Images;
using AIPF.MLManager.Modifiers;
using Microsoft.ML;
using AIPF.Models.Taxi;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.TaxiFare;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF
{
    public class ConsoleExample
    {
        private static string dir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

        private static MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager = new MLManager<RawStringTaxiFare, PredictedFareAmount>();

        public static void TaxiFareExampleConsole()
        {

            /*
                        AnsiConsole.Write(
                            new FigletText("AIPF")
                                .Centered()
                                .Color(Color.Cyan2));*/
            /*AnsiConsole.WriteLine(@"       
                       _____ _____  ______ 
                 /\   |_   _|  __ \|  ____|
                /  \    | | | |__) | |__   
               / /\ \   | | |  ___/|  __|  
              / ____ \ _| |_| |    | |     
             /_/    \_\_____|_|    |_|      v1.1
       Cacchiarelli, Cesetti, Romagnoli 17/12/2021
            ");*/

            //AnsiConsole.Markup("[bold]bold[/] [underline]underline[/] [invert]invert[/]");

            string line = string.Empty;
            while (!line.Equals("exit"))
            {
                line = defaultText();

                try
                {
                    switch (line)
                    {
                        case "train":
                            train(mlManager);
                            AnsiConsole.WriteLine();
                            break;
                        case "predict":
                            predict(mlManager);
                            AnsiConsole.WriteLine();
                            break;
                        case "metrics":
                            metrics(mlManager);
                            AnsiConsole.WriteLine();
                            break;
                        case "exit":
                            break;
                        default:
                            AnsiConsole.WriteLine("[red]Command not found![/]");
                            break;
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
                if (line.Equals("exit")) break;

                AnsiConsole.Write(new Rule("--").RuleStyle("blue").Centered());

                if (!AnsiConsole.Confirm("Continue?"))
                    break;
                AnsiConsole.Clear();
            }
        }

        private static string defaultText()
        {
            var commands = new string[] { "train", "predict", "metrics", "exit" };

            AnsiConsole.Write(
                new FigletText("AIPF")
                    .Centered()
                    .Color(Color.Blue));
            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(commands.Length)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(commands));
        }

        private static void train(MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager)
        {
            AnsiConsole.Write(new Rule("[yellow]Training[/]").RuleStyle("grey").LeftAligned());
            mlManager.CreatePipeline()
                .AddFilter(new MissingPropertyFilter<RawStringTaxiFare>())
                .AddFilter(i => i.PassengersCount >= 1 && i.PassengersCount <= 10)
                .AddTransformer(new GenericDateParser<RawStringTaxiFare, float, MinutesTaxiFare>("yyyy-MM-dd HH:mm:ss UTC", IDateParser<float>.ToMinute))
                .Append(new EuclideanDistance<MinutesTaxiFare, ProcessedTaxiFare>())
                .Build()
                .AddFilter(i => i.Distance > 0 && i.Distance <= 0.5)
                .AddTransformer(new ConcatenateColumn<ProcessedTaxiFare>("input", nameof(ProcessedTaxiFare.Date), nameof(ProcessedTaxiFare.Distance), nameof(ProcessedTaxiFare.PassengersCount)))
                .Append(new ApplyOnnxModel<ProcessedTaxiFare, object>($"{dir}/Data/TaxiFare/Onnx/skl_pca.onnx"))
                .Append(new DeleteColumn<object>("input"))
                .Append(new RenameColumn2<object>("variable", "input"))
                .Append(new DeleteColumn<object>("variable"))
                .Append(new ApplyOnnxModel<object, PredictedFareAmount>($"{dir}/Data/TaxiFare/Onnx/skl_pca_linReg.onnx"))
                .Build();

            var data = new RawStringTaxiFare[] { };
            mlManager.Fit(data, out var dataView);

            AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                    {
                        new TaskDescriptionColumn(),            // Task description
                        new ProgressBarColumn(),                // Progress bar
                        new PercentageColumn(),                 // Percentage
                        new SpinnerColumn(),  // Spinner
                    })
                .Start(ctx =>
                {
                    var random = new Random(DateTime.Now.Millisecond);
                    var task1 = ctx.AddTask("Preparing pipeline");
                    var task2 = ctx.AddTask("Fitting model", autoStart: false).IsIndeterminate();

                    while (!ctx.IsFinished)
                    {
                        task1.Increment(10 * random.NextDouble());
                        Thread.Sleep(75);
                    }

                    task2.StartTask();
                    task2.IsIndeterminate(false);
                    while (!ctx.IsFinished)
                    {
                        task2.Increment(8 * random.NextDouble());
                        Thread.Sleep(75);
                    }
                });

            AnsiConsole.WriteLine("Train complete");
        }

        private static void predict(MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager)
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
            table.AddColumn("Fare amount");


            var predictedValue = mlManager.Predict(toPredict);


            var values = new string[]{
                pickupDatetime,
                pickup_longitude.ToString(),
                pickup_latitude.ToString(),
                dropoff_longitude.ToString(),
                dropoff_latitude.ToString(),
                passenger_count.ToString(),
                predictedValue.FareAmount[0].ToString()
            };

            table.AddRow(values);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
        }

        private static void metrics(MLManager<RawStringTaxiFare, PredictedFareAmount> mlManager)
        {
            var metrics = mlManager.EvaluateAll(mlManager.Loader.LoadFile($"{dir}/Data/TaxiFare/train_mini.csv"));
            if (metrics.Count == 0 || true)
            {
                AnsiConsole.WriteLine("No available metrics.");
            }
            else
            {
                metrics.ForEach(m => AnsiConsole.WriteLine(m.ToString()));
            }
        }
    }
}
