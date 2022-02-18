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
    public class TelemetryTracer
    {
        public static readonly string SERVICE_NAME = "Company.Product.AIPF";
        public static readonly string SERVICE_VERSION = "1.0.0";

        public static readonly string IP = "http://192.168.178.167:4317";
        //"http://localhost:4317"

        public static MeterProvider InizializeMeterProvider()
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
                    opt.Endpoint = new Uri(IP);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();
        }

        public static TracerProvider InitializeTracerProvider()
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
                    opt.Endpoint = new Uri(IP);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                })
                .Build();
        }

        public static ILoggerFactory InitializeLoggerProvider()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            return LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options => options.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = new Uri(IP);
                    opt.Protocol = OtlpExportProtocol.Grpc;
                }));
            });
        }
    }
}
