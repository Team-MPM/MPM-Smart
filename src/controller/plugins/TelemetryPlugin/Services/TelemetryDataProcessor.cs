using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Shared.Services.Telemetry;
using TelemetryPlugin.Data;
using Metric = OpenTelemetry.Metrics.Metric;
using MetricType = OpenTelemetry.Metrics.MetricType;

namespace TelemetryPlugin.Services;

public class TelemetryDataProcessor(
    TelemetryDataCollector dataCollector,
    ILogger<TelemetryDataProcessor> logger,
    IServiceProvider serviceProvider
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

    private async Task ProcessLogRecord(LogRecord record)
    {

    }

    private Task ProcessMetric(Metric metric) =>
        metric.MetricType switch
        {
            MetricType.LongSum or MetricType.DoubleSum or MetricType.LongSumNonMonotonic
                or MetricType.DoubleSumNonMonotonic => ProcessSumMetric(metric),
            MetricType.LongGauge or MetricType.DoubleGauge => ProcessGaugeMetric(metric),
            MetricType.Histogram or MetricType.ExponentialHistogram => ProcessHistogramMetric(metric),
            _ => Task.CompletedTask
        };

    private static async Task EnsureMetricExists(Metric metric, TelemetryDbContext dbContext)
    {
        var metricEntry = await dbContext.Metrics
            .Where(m => m.Name == metric.Name)
            .FirstOrDefaultAsync();

        if (metricEntry is not null)
            return;

        metricEntry = new Data.Metric
        {
            Name = metric.Name,
            MetricName = metric.MeterName,
            Description = metric.Description,
            Unit = metric.Unit,
            Type = metric.MetricType switch
            {
                MetricType.Histogram => Data.MetricType.LinearHistogram,
                MetricType.ExponentialHistogram => Data.MetricType.ExponentialHistogram,
                MetricType.LongGauge or MetricType.DoubleGauge => Data.MetricType.Gauge,
                MetricType.LongSum or MetricType.DoubleSum or MetricType.LongSumNonMonotonic
                    or MetricType.DoubleSumNonMonotonic => Data.MetricType.Counter,
                _ => throw new InvalidEnumArgumentException()
            }
        };

        dbContext.Metrics.Add(metricEntry);
        await dbContext.SaveChangesAsync();
    }
    
    private async Task ProcessHistogramMetric(Metric metric)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
        
        await EnsureMetricExists(metric, dbContext);
        
        foreach (var metricPoint in metric.GetMetricPoints())
        {
            foreach (var histogramBucket in metricPoint.GetHistogramBuckets())
            {
                
            }
        }
    }

    private async Task ProcessGaugeMetric(Metric metric)
    {
    }

    private async Task ProcessSumMetric(Metric metric)
    {
        
    }

    private async Task ProcessTrace(Activity activity)
    {

    }
}