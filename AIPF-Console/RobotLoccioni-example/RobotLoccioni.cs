using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.Date;
using AIPF.MLManager.Modifiers.Maths;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF.Models.Taxi;
using AIPF_Console.RobotLoccioni_example.Model;
using Microsoft.ML;
using Spectre.Console;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace AIPF_Console.RobotLoccioni_example
{
    public class RobotLoccioni : IExample
    {
        private MLManager<RobotData, OutputMeasure> mlManager = new MLManager<RobotData, OutputMeasure>();

        protected RobotLoccioni()
        {

        }

        public static RobotLoccioni Start()
        {
            return new RobotLoccioni();
        }

        public string GetName()
        {
            return "robot";
        }

        public void Train()
        {
            AnsiConsole.Write(new Rule("[yellow]Training[/]").RuleStyle("grey").LeftAligned());

            var propertiesName = typeof(RobotData).GetProperties().Where(p => p.Name.Contains("Axis")).Select(p => p.Name).ToArray();

            mlManager.CreatePipeline()
                //.AddFilter(new MissingPropertyFilter<RobotData>())
                //.AddFilter(i => i.EventType != 0)
                .AddTransformer(new ConcatenateColumn<RobotData>("float_input", propertiesName))
                .Append(new ApplyOnnxModel<RobotData, OutputMeasure>($"{IExample.Dir}/RobotLoccioni-example/Data/Onnx/modello_correnti_robot.onnx"))
                //.Append(new RenameColumn<object>("output_probability", "output_probability"))
                .Build();

            var data = new RobotData[] { };
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

        public void Predict()
        {

            AnsiConsole.Write(new Rule("[yellow]Predicting[/]").RuleStyle("grey").LeftAligned());

            var datetime = AnsiConsole.Ask<string>("Insert the datetime (must be of the format YYYY-MM-DD hh:mm:ss.nnn) ", "2019-07-10 20:50:53.247");
            var maxCurrentAxis1 = AnsiConsole.Ask("Insert the max current axis 1 ", 1.812f);
            var maxCurrentAxis2 = AnsiConsole.Ask<float>("Insert the max current axis 2 ", 10.042f);
            var maxCurrentAxis3 = AnsiConsole.Ask<float>("Insert the max current axis 3 ", 4.116f);
            var maxCurrentAxis4 = AnsiConsole.Ask<float>("Insert the max current axis 4 ", 1.248f);
            var maxCurrentAxis5 = AnsiConsole.Ask<float>("Insert the max current axis 5 ", 1.504f);
            var maxCurrentAxis6 = AnsiConsole.Ask<float>("Insert the max current axis 6 ", 0.853f);
            var rmsCurrentAxis1 = AnsiConsole.Ask<float>("Insert the average current axis 1 ", 1.282f);
            var rmsCurrentAxis2 = AnsiConsole.Ask<float>("Insert the average current axis 2 ", 7.101f);
            var rmsCurrentAxis3 = AnsiConsole.Ask<float>("Insert the average current axis 3 ", 2.911f);
            var rmsCurrentAxis4 = AnsiConsole.Ask<float>("Insert the average current axis 4 ", 0.882f);
            var rmsCurrentAxis5 = AnsiConsole.Ask<float>("Insert the average current axis 5 ", 1.064f);
            var rmsCurrentAxis6 = AnsiConsole.Ask<float>("Insert the average current axis 6 ", 0.603f);

            var toPredict = new RobotData()
            {
                DateAsString = datetime,
                MaxCurrentAxis1 = maxCurrentAxis1,
                MaxCurrentAxis2 = maxCurrentAxis2,
                MaxCurrentAxis3 = maxCurrentAxis3,
                MaxCurrentAxis4 = maxCurrentAxis4,
                MaxCurrentAxis5 = maxCurrentAxis5,
                MaxCurrentAxis6 = maxCurrentAxis6,
                RMSCurrentAxis1 = rmsCurrentAxis1,
                RMSCurrentAxis2 = rmsCurrentAxis2,
                RMSCurrentAxis3 = rmsCurrentAxis3,
                RMSCurrentAxis4 = rmsCurrentAxis4,
                RMSCurrentAxis5 = rmsCurrentAxis5,
                RMSCurrentAxis6 = rmsCurrentAxis6,
            };

            var table = new Table().Centered();
            table.AddColumn("Datetime");
            table.AddColumn("Max Current Axis1");
            table.AddColumn("Max Current Axis2");
            table.AddColumn("Max Current Axis3");
            table.AddColumn("Max Current Axis4");
            table.AddColumn("Max Current Axis5");
            table.AddColumn("Max Current Axis6");
            table.AddColumn("RMS Current Axis1");
            table.AddColumn("RMS Current Axis2");
            table.AddColumn("RMS Current Axis3");
            table.AddColumn("RMS Current Axis4");
            table.AddColumn("RMS Current Axis5");
            table.AddColumn("RMS Current Axis6");
            table.AddColumn("[red]Event Type[/]");
            table.AddColumn("[red]Probability[/]");

            
            var predictedValue = mlManager.Predict(toPredict);
            var sortedDict = from entry in predictedValue.Probability.ToArray()[0] orderby entry.Value descending select entry;

            var values = new string[]{
                datetime,
                maxCurrentAxis1.ToString(),
                maxCurrentAxis2.ToString(),
                maxCurrentAxis3.ToString(),
                maxCurrentAxis4.ToString(),
                maxCurrentAxis5.ToString(),
                maxCurrentAxis6.ToString(),
                rmsCurrentAxis1.ToString(),
                rmsCurrentAxis2.ToString(),
                rmsCurrentAxis3.ToString(),
                rmsCurrentAxis4.ToString(),
                rmsCurrentAxis5.ToString(),
                rmsCurrentAxis6.ToString(),
                //$"[red]{sortedDict.ToArray()[0].Key}[/]",
                $"[red]{predictedValue.EventTypeName()}[/]",
                $"[red]{sortedDict.ToArray()[0].Value}[/]"
            };

            table.AddRow(values);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
        }

        public void Metrics()
        {
            var metrics = mlManager.EvaluateAll(mlManager.Loader.LoadFile($"{IExample.Dir}/RobotLoccioni-example/Data/Dati.csv"));
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

