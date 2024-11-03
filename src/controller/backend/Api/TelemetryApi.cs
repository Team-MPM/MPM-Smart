using Backend.Services;
using Backend.Services.Telemetry;

namespace Backend.Api;

public static class TelemetryApi
{
    public static void MapTelemetryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/telemetry/metrics",
            (TelemetryDataProvider telemetryData) => telemetryData.GetMetricsEntries());

        endpoints.MapGet("/api/telemetry/logs",
            (TelemetryDataProvider telemetryData) => telemetryData.GetLogEntries());

        endpoints.MapGet("/api/telemetry/traces",
            (TelemetryDataProvider telemetryData) => telemetryData.GetTraceEntries());
    }
}