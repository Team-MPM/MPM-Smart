using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Channels;
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

        await Task.WhenAll(
            ProcessTaskTask(logChannel, ProcessLogRecord, cancellationToken),
            ProcessTaskTask(metricChannel, ProcessMetric, cancellationToken),
            ProcessTaskTask(traceChannel, ProcessTrace, cancellationToken));
    }

    private async Task ProcessTaskTask<T>(ChannelReader<T> reader, Func<T, Task> handler,
        CancellationToken cancellationToken)
    {
        await foreach (var entry in reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await handler(entry);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error processing telemetry data");
            }
        }
    }

    private Task ProcessLogRecord(LogRecord record)
    {
        return Task.CompletedTask;
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

    private static async Task<Data.Metric> EnsureMetricExists(Metric metric, TelemetryDbContext dbContext)
    {
        var metricEntry = await dbContext.Metrics
            .Where(m => m.Name == metric.Name)
            .FirstOrDefaultAsync();

        if (metricEntry is not null)
            return metricEntry;

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
        return metricEntry;
    }

    private async Task ProcessHistogramMetric(Metric metric)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        var metricEntry = await EnsureMetricExists(metric, dbContext);

        foreach (var metricPoint in metric.GetMetricPoints())
        {
            foreach (var histogramBucket in metricPoint.GetHistogramBuckets())
            {
                dbContext.HistogramBucketMetricEntries.Add(new HistogramBucketMetricEntry
                {
                    MetricId = metricEntry.Id,
                    Bucket = histogramBucket.ExplicitBound,
                    Count = histogramBucket.BucketCount,
                    TimeStampStartUtc = metricPoint.StartTime.UtcDateTime,
                    TimeStampEndUtc = metricPoint.EndTime.UtcDateTime
                });
            }

            dbContext.HistogramSumMetricEntries.Add(new HistogramSumMetricEntry
            {
                MetricId = metricEntry.Id,
                Sum = metricPoint.GetHistogramSum(),
                TimeStampStartUtc = metricPoint.StartTime.UtcDateTime,
                TimeStampEndUtc = metricPoint.EndTime.UtcDateTime
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task ProcessGaugeMetric(Metric metric)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        var metricEntry = await EnsureMetricExists(metric, dbContext);

        foreach (var metricPoint in metric.GetMetricPoints())
        {
            dbContext.GaugeMetricEntries.Add(new GaugeMetricEntry
            {
                MetricId = metricEntry.Id,
                Value = metricPoint.GetGaugeLastValueDouble(),
                TimeStampStartUtc = metricPoint.StartTime.UtcDateTime,
                TimeStampEndUtc = metricPoint.EndTime.UtcDateTime
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task ProcessSumMetric(Metric metric)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        var metricEntry = await EnsureMetricExists(metric, dbContext);

        foreach (var metricPoint in metric.GetMetricPoints())
        {
            dbContext.CounterMetricEntries.Add(new CounterMetricEntry
            {
                MetricId = metricEntry.Id,
                Value = metricPoint.GetSumLong(),
                TimeStampStartUtc = metricPoint.StartTime.UtcDateTime,
                TimeStampEndUtc = metricPoint.EndTime.UtcDateTime
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private Task ProcessTrace(Activity activity)
    {
        return Task.CompletedTask;
    }
}