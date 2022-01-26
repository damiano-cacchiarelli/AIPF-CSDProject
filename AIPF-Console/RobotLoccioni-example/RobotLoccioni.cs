using AIPF.MLManager;
using AIPF.MLManager.Actions.Filters;
using AIPF.MLManager.Metrics;
using AIPF.MLManager.Modifiers;
using AIPF.MLManager.Modifiers.Columns;
using AIPF.MLManager.Modifiers.TaxiFare;
using AIPF_Console.RobotLoccioni_example.Model;
using AIPF_Console.Utils;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIPF_Console.RobotLoccioni_example
{
    public class RobotLoccioni : IExample
    {
        private static RobotLoccioni instance = new RobotLoccioni();
        private MLManager<RobotData, OutputMeasure> mlManager = new MLManager<RobotData, OutputMeasure>();

        public string Name => "Robot-Loccioni";

        protected RobotLoccioni()
        {

        }

        public static RobotLoccioni Start()
        {
            return instance;
        }


        public async Task Train()
        {
            AnsiConsole.Write(new Rule("[yellow]Training[/]").RuleStyle("grey").LeftAligned());

            if (Program.REST)
            {
                dynamic fitBody = new { ModelName = Name, Data = new object[0] };
                RestService.Post<string>("train", fitBody);
            }
            else
            {
                var propertiesName = typeof(RobotData).GetProperties().Where(p => p.Name.Contains("Axis")).Select(p => p.Name).ToArray();

                mlManager.CreatePipeline()
                    .AddTransformer(new ProgressIndicator<RobotData>($"{Name}Process#1"))
                    //.AddFilter(new MissingPropertyFilter<RobotData>())
                    //.AddFilter(i => i.EventType != 0)
                    .Append(new ConcatenateColumn<RobotData>("float_input", propertiesName))
                    .Append(new ApplyEvaluableOnnxModel<RobotData, OutputMeasure, MulticlassEvaluate>(
                        $"{IExample.Dir}/RobotLoccioni-example/Data/Onnx/modello_correnti_robot.onnx",
                        (i, o) => 
                        {
                            o.PredictedEventType = i.EventType[0];
                            o.ProbabilityEventType = i.GetProbability();
                            //AnsiConsole.WriteLine("processing...");
                        }))
                    .Build();

                var data = new RobotData[] { };
                var fitTask = mlManager.Fit(data);

                await ConsoleHelper.Loading("Fitting model", $"{Name}Process#1");
                await fitTask;
            }

            AnsiConsole.WriteLine("Train complete");
        }

        public async Task Predict()
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

            OutputMeasure predictedValue;
            if (Program.REST)
            {
                predictedValue = RestService.Put<OutputMeasure>($"predict/{Name}", toPredict).Result;
            }
            else
            {
                predictedValue = await mlManager.Predict(toPredict);
            }

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
                $"[red]{predictedValue.EventTypeName()}[/]",
                $"[red]{(predictedValue.Probability.ToArray()[0])[predictedValue.EventType[0]]}[/]"
            };

            table.AddRow(values);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
        }

        public async Task Metrics()
        {
            var metrics = new List<MetricContainer>();
            var data = mlManager.Loader.LoadFile($"{IExample.Dir}/RobotLoccioni-example/Data/Dati.csv", ';');
            if (Program.REST)
            {
                var rawDataList = mlManager.Loader.GetEnumerable(data).Take(50);
                dynamic fitBody = new { ModelName = Name, Data = rawDataList };
                metrics = RestService.Put<List<MetricContainer>>("metrics", fitBody).Result;
            }
            else
            {
                var rawDataList = mlManager.Loader.GetEnumerable(data).Take(5);
                var taskMetrics = mlManager.EvaluateAll(rawDataList);
                await ConsoleHelper.Loading("Evaluating model", $"{Name}Process#1");
                metrics = await taskMetrics;
            }

            ConsoleHelper.PrintMetrics(metrics);
        }
    }
}

