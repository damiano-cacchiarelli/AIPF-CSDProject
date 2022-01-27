using OpenTelemetry;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AIPF_Console.Telemetry.Exporters.Console
{
    
    class CustomConsoleExporter: BaseExporter<Activity>
    {
        public static List<string> logs = new List<string>();
        private readonly string name;

        public CustomConsoleExporter(string name = "CustomConsoleExporter")
        {
            this.name = name;
        }

        public override ExportResult Export(in Batch<Activity> batch)
        {
            // SuppressInstrumentationScope should be used to prevent exporter
            // code from generating telemetry and causing live-loop.
            using var scope = SuppressInstrumentationScope.Begin();

            var sb = new StringBuilder();
            foreach (var activity in batch)
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(activity.DisplayName);
            }

            logs.Add($"{this.name}.Export([{sb.ToString()}])");
            //AnsiConsole.WriteLine($"{this.name}.Export([{sb.ToString()}])");

            return ExportResult.Success;
        }

        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            logs.Add($"{this.name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})");
            //AnsiConsole.WriteLine($"{this.name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})");
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            logs.Add($"{this.name}.Dispose({disposing})");
            //AnsiConsole.WriteLine($"{this.name}.Dispose({disposing})");
        }
    }
}
