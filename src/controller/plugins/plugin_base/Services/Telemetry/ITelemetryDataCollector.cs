using System.Diagnostics;
using System.Threading.Channels;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace PluginBase.Services.Telemetry;

public interface ITelemetryDataCollector
{
    public Channel<LogRecord> LogRecordChannel { get; set; }
    public Channel<Metric> MetricsChannel { get; set; }
    public Channel<Activity> TraceChannel { get; set; }
}