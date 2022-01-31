using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using System;
using System.Collections.Generic;

namespace AIPF.Telemetry
{
    public class TelemetryTracer
    {
        /* private static Telemetry _instance;



        public static Telemetry Instance
        {
        get
        {
        if (_instance == null) _instance = new Telemetry();
        return _instance;
        }
        }
        */

        public static readonly string SERVICE_NAME = "Company.Product.AIPF";
        public static readonly string SERVICE_VERSION = "1.0.0";



        public static Dictionary<string, string> CONSTANTS { get; internal set; }



        //public static readonly ActivitySource @ActivitySource = new ActivitySource("MLManager");
        //public static readonly ActivitySource @ActivitySource2 = new ActivitySource("MLManager2");

        public static MeterProvider InizializeMeterProvider()
        {
            var builder = Sdk.CreateMeterProviderBuilder()
                .AddMeter("MyCompany.MyProduct.MyLibrary")
                .AddConsoleExporter()
                .Build();

            return builder;
        }

        public static TracerProvider InitializeTracer(Func<TracerProviderBuilder, TracerProviderBuilder> exporter)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var builder = Sdk.CreateTracerProviderBuilder()
            .AddSource("MLManager")
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                .AddService(serviceName: SERVICE_NAME, serviceVersion: SERVICE_VERSION))
            

            //return exporter.Invoke(builder)
            //.AddConsoleExporter()
            .AddOtlpExporter(opt => {
                opt.Endpoint = new Uri("http://localhost:4317");
                opt.Protocol = OtlpExportProtocol.Grpc;
            })
            .Build();
            return builder;
            /*
            return Sdk.CreateTracerProviderBuilder()
            .AddSource("MLManager")
            .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
            .AddService(serviceName: SERVICE_NAME, serviceVersion: SERVICE_VERSION))
            .AddConsoleExporter()
            //.AddOtlpExporter(opt => opt.Endpoint = new Uri(endpoint))
            .Build();
            */
        }
    }
}
