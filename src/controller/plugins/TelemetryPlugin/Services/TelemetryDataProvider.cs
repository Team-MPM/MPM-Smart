using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using PluginBase.Services.Telemetry;

namespace TelemetryPlugin.Services;

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