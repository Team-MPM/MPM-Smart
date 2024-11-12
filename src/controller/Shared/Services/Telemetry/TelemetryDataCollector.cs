using System.Diagnostics;
using System.Threading.Channels;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Shared.Services.Telemetry;

public class TelemetryDataCollector
{
    public TelemetryCollection<LogRecord> LogRecords { get; } = [];
    public TelemetryCollection<Metric> Metrics { get; } = [];
    public TelemetryCollection<Activity> Traces { get; } = [];

    public Channel<LogRecord> LogRecordChannel { get; set; }
    public Channel<Metric> MetricsChannel { get; set; }
    public Channel<Activity> TraceChannel { get; set; }

    public TelemetryDataCollector()
    {
        LogRecordChannel = Channel.CreateUnbounded<LogRecord>();
        MetricsChannel = Channel.CreateUnbounded<Metric>();
        TraceChannel = Channel.CreateUnbounded<Activity>();

        LogRecords.ItemsAdded += async record =>
            await LogRecordChannel.Writer.WriteAsync(record);

        Metrics.ItemsAdded += async metric =>
            await MetricsChannel.Writer.WriteAsync(metric);

        Traces.ItemsAdded += async activity =>
            await TraceChannel.Writer.WriteAsync(activity);
    }
}