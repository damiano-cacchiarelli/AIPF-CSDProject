using System;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace AIPF_Console.Telemetry.Exporters.Console
{
    internal static class CustomConsoleExporterExtensions
    {
        public static TracerProviderBuilder AddMyExporter(this TracerProviderBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.AddProcessor(new BatchActivityExportProcessor(new CustomConsoleExporter()));
        }
    }
}
