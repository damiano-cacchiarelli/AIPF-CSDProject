using AIPF.Telemetry;
using AIPF_Console.Utils;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Threading.Tasks;

namespace AIPF_Console
{
    class Program
    {
        private static readonly Meter PerformanceMeter = new Meter("Performance", TelemetryManager.SERVICE_VERSION);

        public static bool REST => Config.rest;
        private static Config config;
        public static Config Config
        {
            get
            {
                if (config == null)
                {
                    using (StreamReader r = new StreamReader("config.json"))
                    {
                        string json = r.ReadToEnd();
                        config = JsonConvert.DeserializeObject<Config>(json);
                    }
                }
                return config;
            }
        }

        public async static Task Main(string[] args)
        {
            using var sdkMeterProvider = TelemetryManager.InizializeMeterProvider(Config.ip);
            var process = Process.GetCurrentProcess();
            PerformanceMeter.CreateObservableCounter("Thread.CpuTime", () => ProfilerHelper.GetThreadCpuTime(process), "ms");
            PerformanceMeter.CreateObservableGauge("Thread.State", () => ProfilerHelper.GetThreadState(process));
            PerformanceMeter.CreateObservableGauge("system.process.memory.size", () => ProfilerHelper.GetMemoryUsage());
            PerformanceMeter.CreateObservableGauge("system.cpu.total.norm.pct", () => ProfilerHelper.GetCPUUsage(0.01f));

            using (var sdkTracerProvider = TelemetryManager.InitializeTracerProvider(Config.ip))
            {
                if (Config.consoleType.Equals("manual")) await ManualConsoleApplication.Instance.Start();
                else await AutoConsoleApplication.Instance.Start();
            }
        }
    }
}