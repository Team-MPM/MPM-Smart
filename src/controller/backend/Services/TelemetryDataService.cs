using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Backend.Services;

public class TelemetryDataService
{
    public List<LogRecord> LogRecords { get; } = [];
    public List<Metric> Metrics { get; } = [];
    public List<Activity> Traces { get; } = [];

    public IEnumerable<LogRecord> GetLogRecords() => LogRecords;
    public IEnumerable<Metric> GetMetrics() => Metrics;
    public IEnumerable<Activity> GetTraces() => Traces;

    public void ClearLogRecords() => LogRecords.Clear();
    public void ClearMetrics() => Metrics.Clear();
    public void ClearTraces() => Traces.Clear();
}