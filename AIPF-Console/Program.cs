using AIPF.Telemetry;
using AIPF_Console.MNIST_example;
using AIPF_Console.RobotLoccioni_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace AIPF_Console
{
    class Program
    {
        private static readonly Meter MyMeter = new Meter("MyCompany.MyProduct.MyLibrary", "1.0");
        private static readonly Histogram<long> MyHistogram = MyMeter.CreateHistogram<long>("MyHistogram");

        public static readonly bool REST = false;
        private static IExample example = null;
        private static readonly Dictionary<string, Func<IExample, Task>> Commands = new Dictionary<string, Func<IExample, Task>>()
            {
                { "fit", async e => await e.Train() },
                { "predict", async e => await e.Predict() },
                { "metrics", async e => await e.Metrics() },
                { "back", _ => { example = null; return Task.CompletedTask; } },
                { "exit", _ => Task.CompletedTask },
            };        

        public async static Task Main(string[] args)
        {
            string line = string.Empty;
            using (var sdk = TelemetryTracer.InizializeMeterProvider())
            {
                var process = Process.GetCurrentProcess();

                MyMeter.CreateObservableCounter("Thread.CpuTime", () => GetThreadCpuTime(process), "ms");

                MyMeter.CreateObservableGauge("Thread.State", () => GetThreadState(process));

                var random = new Random();
                for (int i = 0; i < 1000; i++)
                {
                    MyHistogram.Record(random.Next(1, 1000));
                }
            }
            /*
            using (var skd = TelemetryTracer.InitializeTracer(null))
            {
                while (!line.Equals("exit"))
                {
                    if (example == null)
                    {
                        example = SelectExample();
                    }
                    line = DefaultText();

                    try
                    {
                        await Commands[line].Invoke(example);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.WriteException(ex);
                    }
                    if (line.Equals("exit")) break;

                    if (!line.Equals("back"))
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.Write(new Rule("--").RuleStyle("blue").Centered());

                        if (!AnsiConsole.Confirm("Continue?"))
                            break;
                    }
                    AnsiConsole.Clear();
                }
            }*/
        }

        private static IExample SelectExample()
        {
            var examples = new Dictionary<string, Func<IExample>>()
            {
                { "mnist", () => Mnist.Start() },
                { "taxi-fare", () => TaxiFare.Start() },
                { "robot-loccioni", () => RobotLoccioni.Start() }
            };

            var command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(examples.Count)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(examples.Keys));

            return examples[command].Invoke();
        }

        private static string DefaultText()
        {
            AnsiConsole.Write(
                new FigletText("AIPF - " + example.Name)
                    .Centered()
                    .Color(Color.Blue));

            AnsiConsole.Write(new Rule("[bold white]Cacchiarelli, Cesetti, Romagnoli 10/01/2022[/]").RuleStyle("blue").Centered());
            AnsiConsole.WriteLine();

            return AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select the command you what to do")
                    .PageSize(Commands.Count)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(Commands.Keys));
        }

        private static IEnumerable<Measurement<double>> GetThreadCpuTime(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                yield return new Measurement<double>(thread.TotalProcessorTime.TotalMilliseconds, new KeyValuePair<string, object?>("ProcessId", process.Id),
                    new KeyValuePair<string, object?>("ThreadId", thread.Id));
            }
        }

        private static IEnumerable<Measurement<char>> GetThreadState(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                yield return new Measurement<char>(thread.ThreadState.ToString().ToCharArray()[0], new KeyValuePair<string, object?>("ProcessId", process.Id), new KeyValuePair<string, object?>("ThreadId", thread.Id));
            }
        }
    }
}
