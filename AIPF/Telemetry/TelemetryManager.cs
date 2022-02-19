using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace AIPF.Telemetry
{
    public class TelemetryManager
    {
        public static readonly string SERVICE_NAME = "Loccioni.CSD.AIPF";
        public static readonly string SERVICE_VERSION = "1.0.0";

        public static MeterProvider InizializeMeterProvider(string ip)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            return Sdk.CreateMeterProviderBuilder()
                .AddMeter("Performance")
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                    .AddTelemetrySdk()
                    .AddService(serviceName: SERVICE_NAME, serviceVersion: SERVICE_VERSION))
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = GetUri(ip);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();
        }

        public static TracerProvider InitializeTracerProvider(string ip)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            return Sdk.CreateTracerProviderBuilder()
                .AddSource("MLManager", "PipelineBuilder", "Filter", "Transformer")
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                    .AddTelemetrySdk()
                    .AddService(serviceName: SERVICE_NAME, serviceVersion: SERVICE_VERSION))
                .AddOtlpExporter(opt =>
                {
                    opt.Endpoint = GetUri(ip);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();
        }

        public static ILoggerFactory InitializeLoggerProvider(string ip)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            return LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options => options.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = GetUri(ip);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                }));
            });
        }

        private static Uri GetUri(string ip)
        {
            return new Uri($"http://{ip}:4317");
        }
    }
}
