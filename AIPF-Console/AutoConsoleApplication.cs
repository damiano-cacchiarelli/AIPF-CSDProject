using AIPF_Console.RobotLoccioni_example;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF_Console
{
    public class AutoConsoleApplication : AbstractConsoleApplication
    {
        private static AutoConsoleApplication instance;
        public static AutoConsoleApplication Instance
        {
            get
            {
                if (instance == null) instance = new AutoConsoleApplication();
                return instance;
            }
        }
        private bool wait = false;

        private int waitAfterXExamples = 100; 

        protected AutoConsoleApplication() { }

        public override async Task Start()
        {
            var exitTask = Task.Run(() => Exit());
            Welcome();

            TaskConfig taskConf;
            int runnedExamples = 0;
            while (!exitTask.IsCompleted)
            {
                if (!wait)
                {
                    AnsiConsole.Write(new Rule("Selecting the context...").RuleStyle("blue").Centered());
                    example = SelectExample();
                    AnsiConsole.WriteLine($"{example.Name} selected");
                    AnsiConsole.WriteLine();
                    Thread.Sleep(IExample.random.Next(100, 200));
                    AnsiConsole.Write(new Rule("Generating task config...").RuleStyle("blue").Centered());
                    taskConf = NewTaskConfig();
                    AnsiConsole.WriteLine($"{taskConf} generated");
                    AnsiConsole.WriteLine();
                    Thread.Sleep(IExample.random.Next(2000, 2500));
                    AnsiConsole.Write(new Rule("Starting OpenTelemetry activity!").RuleStyle("green").Centered());
                    await example.OpenTelemetry(taskConf);
                    
                    runnedExamples++;
                    /*
                    if(runnedExamples > waitAfterXExamples)
                    {
                        Thread.Sleep(IExample.random.Next(300000, 600000));
                        runnedExamples = 0;
                        waitAfterXExamples = IExample.random.Next(50, 200);
                    }
                    */

                    if (!exitTask.IsCompleted)
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.WriteLine();
                        AnsiConsole.Write(new Rule("OpenTelemetry activity finished! Starting a new Context...").RuleStyle("red").Centered());
                        AnsiConsole.WriteLine($"Already processed context: {runnedExamples}");
                        AnsiConsole.WriteLine("Type w to standby, or press any key to stop... (the program will continue forever)");
                        Thread.Sleep(IExample.random.Next(2000, 10000));
                    }
                    AnsiConsole.Clear();
                }
            }

            ExitText();
        }

        protected override IExample SelectExample()
        {
            var rnd = new Random().Next(0, Examples.Count);
            AnsiConsole.WriteLine(rnd);
            return Examples.ElementAt(rnd).Value.Invoke();
        }

        private TaskConfig NewTaskConfig()
        {
            Random random = new Random();
            switch (example.Name)
            {
                case "Robot-Loccioni":
                    return new TaskConfig()
                    {
                        NumberOfPredictions = random.Next(5, 15),
                        NumberOfElementsForEvaluate = random.Next(5, 10)
                    };
                default:
                    return new TaskConfig()
                        {
                            NumberOfIterations = random.Next(50, 500),
                            NumberOfElementsForTrain = random.Next(100, 3000),

                            NumberOfPredictions = random.Next(200, 1000),
                            PercentageOfIncorrectPredictions = random.Next(0, 10),
                            MinMillisecondsToSleepBetweenPredictions = 0,
                            MaxMillisecondsToSleepBetweenPredictions = 50,

                            NumberOfElementsForEvaluate = random.Next(500, 10000)
                    };
            }
        }

        private void Welcome()
        {
            AnsiConsole.Write(
                new FigletText("AIPF - Auto")
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine("The program will start in 5 seconds. Type w to standby, or any other key to stop the application.");
            AnsiConsole.WriteLine();
            Thread.Sleep(5000);
        }

        private void Exit()
        {
            string line = null;
            while (line == null)
            {
                line = Console.ReadLine();
                if (wait)
                {
                    line = null;
                }
                if (line.ToLower().Equals("w"))
                {
                    wait = true;
                    line = null;
                    AnsiConsole.WriteLine("Type r to resume the computation");
                }
                else if (line.ToLower().Equals("r"))
                {
                    wait = false;
                    line = null;
                }
            }
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("The application will stop as soon the operation end!").RuleStyle("magenta").Centered());
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
        }
    }
}
