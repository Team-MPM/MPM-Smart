using Backend.Services;

namespace Backend.Api;

public static class TelemetryApi
{
    public static void MapTelemetryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/telemetry/metrics",
            (TelemetryDataService telemetryDataService) => telemetryDataService.Metrics.Select(m => m));

        endpoints.MapGet("/api/telemetry/logs",
            (TelemetryDataService telemetryDataService) => telemetryDataService.LogRecords.Select(m => m));

        endpoints.MapGet("/api/telemetry/traces",
            (TelemetryDataService telemetryDataService) => telemetryDataService.Traces.Select(m => m));
    }
}