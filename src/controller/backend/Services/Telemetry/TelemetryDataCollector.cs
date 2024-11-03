using System.Collections.ObjectModel;
using System.Diagnostics;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Backend.Services.Telemetry;

public class TelemetryDataCollector
{
    public ObservableCollection<LogRecord> LogRecords { get; } = [];
    public ObservableCollection<Metric> Metrics { get; } = [];
    public ObservableCollection<Activity> Traces { get; } = [];
    
    public void ClearAll()
    {
        LogRecords.Clear();
        Metrics.Clear();
        Traces.Clear();
    }
}