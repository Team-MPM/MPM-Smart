using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using Shared.Services.Telemetry;

namespace TelemetryPlugin.Services;

public class TelemetryDataProcessor(
    TelemetryDataCollector dataCollector,
    ILogger<TelemetryDataProcessor> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var logChannel = dataCollector.LogRecordChannel.Reader;
        var metricChannel = dataCollector.MetricsChannel.Reader;
        var traceChannel = dataCollector.TraceChannel.Reader;

        var logTask = Task.Run(async () =>
        {
            await foreach (var log in logChannel.ReadAllAsync(cancellationToken))
            {
                await ProcessLogRecord(log);
            }
        }, cancellationToken);

        var metricTask = Task.Run(async () =>
        {
            await foreach (var metric in metricChannel.ReadAllAsync(cancellationToken))
            {
                await ProcessMetric(metric);
            }
        }, cancellationToken);

        var traceTask = Task.Run(async () =>
        {
            await foreach (var trace in traceChannel.ReadAllAsync(cancellationToken))
            {
                await ProcessTrace(trace);
            }
        }, cancellationToken);

        await Task.WhenAll(logTask, metricTask, traceTask);
    }

    public async Task ProcessLogRecord(LogRecord record)
    {

    }

    public async Task ProcessMetric(Metric metric)
    {
        metric.
    }

    public async Task ProcessTrace(Activity activity)
    {

    }
}