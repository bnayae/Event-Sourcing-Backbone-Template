using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

// Configuration: https://medium.com/@gparlakov/the-confusion-of-asp-net-configuration-with-environment-variables-c06c545ef732
// see:
//  https://opentelemetry.io/docs/instrumentation/net/getting-started/
//  https://opentelemetry.io/docs/demo/services/cart/
//  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables
//  https://opentelemetry.io/docs/demo/docker-deployment/

namespace Skeleton;

/// <summary>
/// Open telemetry extensions for ASP.NET Core
/// </summary>
internal static class OpenTelemetryExtensions
{
    #region AddOpenTelemetryEventSourcing

    /// <summary>
    /// Adds open telemetry for event sourcing.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IServiceCollection AddOpenTelemetryEventSourcing(this WebApplicationBuilder builder)
    {
        IWebHostEnvironment environment = builder.Environment;
        IServiceCollection services = builder.Services;

        // see:
        //  https://opentelemetry.io/docs/instrumentation/net/getting-started/
        //  https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Jaeger/README.md#environment-variables
        services.AddOpenTelemetry()
                .WithEventSourcingTracing(environment,
                        cfg =>
                        {
                            cfg
                                .AddAspNetCoreInstrumentation(m =>
                                {
                                    m.Filter = TraceFilter;
                                    m.RecordException = true;
                                    m.EnableGrpcAspNetCoreSupport = true;
                                })
                                .AddHttpClientInstrumentation(m =>
                                {
                                    // m.Enrich
                                    m.RecordException = true;
                                })
                                .AddGrpcClientInstrumentation()
                                .AddOtlpExporter();
                            if (environment.IsDevelopment())
                                cfg.AddConsoleExporter();
                        })
                .WithEventSourcingMetrics(environment, cfg =>
                {
                    cfg.AddAspNetCoreInstrumentation( /* m => m.Filter = filter */)
                        .AddOtlpExporter()
                        .AddPrometheusExporter();
                    if (environment.IsDevelopment())
                        cfg.AddConsoleExporter();
                });

        return services;
    }

    #endregion // AddOpenTelemetryEventSourcing

    #region TraceFilter

    /// <summary>
    /// Telemetries the filter.
    /// </summary>
    /// <param name="ctx">The CTX.</param>
    /// <returns></returns>
    private static bool TraceFilter(HttpContext ctx) => ctx.Request.Path.Value switch
    {
        "/health" => false,
        "/readiness" => false,
        "/metrics" => false,
        string x when x.StartsWith("/swagger")  => false,
        string x when x.StartsWith("/_framework/")  => false,
        string x when x.StartsWith("/_vs/")  => false,
        _ => true
    };

    #endregion // TraceFilter
}
