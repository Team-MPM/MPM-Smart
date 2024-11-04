using System.Diagnostics;
using ApiSchema.Telemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Backend.Services.Telemetry;

public class TelemetryDataProvider(TelemetryDataCollector collector)
{
    public IEnumerable<LogRecord> GetLogEntries()
    {
        return collector.LogRecords.ToList();
    }
    
    public IEnumerable<Metric> GetMetricsEntries()
    {
        return collector.Metrics.ToList();
    }
    
    public IEnumerable<Activity> GetTraceEntries()
    {
        return collector.Traces.ToList();
    }
}