using AIPF.Telemetry;
using AIPF_Console.MNIST_example;
using AIPF_Console.RobotLoccioni_example;
using AIPF_Console.TaxiFare_example;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace AIPF_Console
{
    class Program
    {
        private static readonly Meter MyMeter = new Meter(TelemetryTracer.SERVICE_NAME, TelemetryTracer.SERVICE_VERSION);
        private static readonly Histogram<long> MyHistogram = MyMeter.CreateHistogram<long>("MyHistogram");
        private static readonly Histogram<float> MyCpuHistogram = MyMeter.CreateHistogram<float>("MyCpuHistogram");
        private static readonly Counter<long> MyFruitCounter = MyMeter.CreateCounter<long>("MyFruitCounter");

        public static readonly bool REST = false;
        private static IExample example = null;
        private static readonly Dictionary<string, Func<IExample, Task>> Commands = new Dictionary<string, Func<IExample, Task>>()
            {
                { "fit", async e => await e.Train() },
                { "predict", async e => await e.Predict() },
                { "metrics", async e => await e.Metrics() },
                { "opentelemetry", async e =>
                    {
                        await e.Train();
                        var n = AnsiConsole.Ask("Number of predictions ", 100);
                        for (int i = 0; i < n; i++)
                        {
                            await e.Predict(PredictionMode.RANDOM_VALUE);
                        }
                        await e.Metrics();
                    }
                },
                { "back", _ => { example = null; return Task.CompletedTask; } },
                { "exit", _ => Task.CompletedTask },
            };

        private static void ExampleMetricsCpu()
        {
            using (var sdk = TelemetryTracer.InizializeMeterProvider())
            {
                var process = Process.GetCurrentProcess();

                MyMeter.CreateObservableCounter("process.cpu.time", () => GetProcessCpuTime(process), "ms");
                MyMeter.CreateObservableCounter("Thread.CpuTime", () => GetThreadCpuTime(process), "ms");
                MyMeter.CreateObservableGauge("Thread.State", () => GetThreadState(process));

                var random = new Random();

                for (int j = 0; j < 5; j++)
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        MyHistogram.Record(random.Next(1, 1000));
                    }
                    Console.WriteLine("End " + j);
                    Thread.Sleep(1000);
                }

            }
        }

        private static void ExampleOtherMetrics()
        {
            using (var sdk = TelemetryTracer.InizializeMeterProvider())
            {
                Console.WriteLine("Press any key to exit");
                while (!Console.KeyAvailable)
                {
                    MyFruitCounter.Add(1, new KeyValuePair<string, object>("name", "apple"), new KeyValuePair<string, object>("color", "red"));
                    MyFruitCounter.Add(2, new KeyValuePair<string, object>("name", "lemon"), new KeyValuePair<string, object>("color", "yellow"));
                    MyFruitCounter.Add(1, new KeyValuePair<string, object>("name", "lemon"), new KeyValuePair<string, object>("color", "yellow"));
                    MyFruitCounter.Add(2, new KeyValuePair<string, object>("name", "apple"), new KeyValuePair<string, object>("color", "green"));
                    MyFruitCounter.Add(5, new KeyValuePair<string, object>("name", "apple"), new KeyValuePair<string, object>("color", "red"));
                    MyFruitCounter.Add(4, new KeyValuePair<string, object>("name", "lemon"), new KeyValuePair<string, object>("color", "yellow"));
                    Console.WriteLine("Added");
                    Thread.Sleep(2000);
                }
            }
        }

        public async static Task Main(string[] args)
        {
            //ExampleOtherMetrics();

            using var sdkMeterProvider = TelemetryTracer.InizializeMeterProvider();
            var process = Process.GetCurrentProcess();
            //MyMeter.CreateObservableCounter("Thread.CpuTime", () => GetThreadCpuTime(process), "ms");
            MyMeter.CreateObservableCounter("process.cpu.time", () => GetProcessCpuTime(process), "ms");
            //MyMeter.CreateObservableGauge("Thread.State", () => GetThreadState(process));
            //MyMeter.CreateObservableGauge("CPU.percentage", () => GetCPUPercentage(process));

            //_ = CheckCPU(process);

            string line = string.Empty;
            using (var sdkTracerProvider = TelemetryTracer.InitializeTracer(null))
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
            }
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

        private static DateTime lastTime;
        private static TimeSpan lastTotalProcessorTime;
        private static DateTime curTime;
        private static TimeSpan curTotalProcessorTime;

        private static Measurement<double> GetCPUPercentage(Process process)
        {
            double CPUUsage = 0;
            if (!process.HasExited)
            {
                if (lastTime == null || lastTime == new DateTime())
                {
                    lastTime = DateTime.Now;
                    lastTotalProcessorTime = process.TotalProcessorTime;
                }
                else
                {
                    curTime = DateTime.Now;
                    curTotalProcessorTime = process.TotalProcessorTime;

                    CPUUsage = (curTotalProcessorTime.TotalMilliseconds - lastTotalProcessorTime.TotalMilliseconds) / curTime.Subtract(lastTime).TotalMilliseconds / Convert.ToDouble(Environment.ProcessorCount);
                    CPUUsage *= 100;
                    AnsiConsole.WriteLine("{0} CPU: {1:0.0}%", process.ProcessName, CPUUsage);

                    lastTime = curTime;
                    lastTotalProcessorTime = curTotalProcessorTime;
                }
            }

            return new Measurement<double>(CPUUsage);
        }

        private static Measurement<double> GetProcessCpuTime(Process process)
        {
            if (!process.HasExited)
            {
                return new Measurement<double>(process.TotalProcessorTime.TotalMilliseconds);
            }

            return default;
        }

        private static IEnumerable<Measurement<double>> GetThreadCpuTime(Process process)
        {
            if (!process.HasExited)
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    Measurement<double> m = default;
                    try
                    {
                        m = new Measurement<double>(thread.TotalProcessorTime.TotalMilliseconds, new KeyValuePair<string, object?>("ProcessId", process.Id),
                                new KeyValuePair<string, object?>("ThreadId", thread.Id));
                    }
                    catch (Exception) { }

                    yield return m;

                }
            }
        }

        private static IEnumerable<Measurement<int>> GetThreadState(Process process)
        {
            foreach (ProcessThread thread in process.Threads)
            {
                Measurement<int> m = default;
                try
                {
                    m = new Measurement<int>((int)thread.ThreadState, new KeyValuePair<string, object?>("ProcessId", process.Id),
                    new KeyValuePair<string, object?>("ThreadId", thread.Id));
                }
                catch (Exception) { }

                yield return m;
            }
        }

        private static Task CheckCPU(Process process)
        {
            //var cpuCounter = MyMeter.CreateHistogram<double>("CPU.percentage.counter");
            //var ramCounter = MyMeter.CreateCounter<Measurement<double>>("RAM.percentage.counter");

            return Task.Run(() =>
            {
                bool done = false;
                PerformanceCounter total_cpu = new PerformanceCounter("Process", "% Processor Time", "_Total");
                PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                while (!done)
                {
                    float t = total_cpu.NextValue();
                    float p = process_cpu.NextValue();
                    //AnsiConsole.WriteLine(String.Format("_Total = {0}  App = {1} {2}%\n", t, p, p / t * 100));

                    MyCpuHistogram.Record(p);

                    System.Threading.Thread.Sleep(1000);
                }
            });
        }
    }
}
