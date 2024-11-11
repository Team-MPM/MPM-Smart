using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using ApiSchema.Telemetry;
using Data.Telemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using LogEntry = ApiSchema.Telemetry.LogEntry;

namespace Backend.Services.Telemetry;

public class TelemetryDataProcessor(
    TelemetryDataCollector dataCollector,
    ILogger<TelemetryDataProcessor> logger
) : BackgroundService
{
    //private const int MaxBatchSize = 10000;

    private const string MetricsFileName = "logs/metrics.json";
    private const string LogsFileName = "logs/logs.json";
    private const string TracesFileName = "logs/traces.json";

    private FileStream? m_MetricsFileStream;
    private FileStream? m_LogsFileStream;
    private FileStream? m_TracesFileStream;

    private readonly List<MetricsEntry> m_MetricsBatch = [];
    private readonly List<LogEntry> m_LogsBatch = [];
    private readonly List<TraceEntry> m_TracesBatch = [];

    private readonly ManualResetEventSlim m_WriteSignal = new(false);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dataCollector.LogRecords.CollectionChanged += TelemetryData_CollectionChanged;
        dataCollector.Metrics.CollectionChanged += TelemetryData_CollectionChanged;
        dataCollector.Traces.CollectionChanged += TelemetryData_CollectionChanged;

        m_MetricsFileStream =
            new FileStream(MetricsFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        m_LogsFileStream = new FileStream(LogsFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        m_TracesFileStream =
            new FileStream(TracesFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

        m_WriteSignal.Set();

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
            await WriteToDisk(m_LogsBatch, m_LogsFileStream);
            await WriteToDisk(m_MetricsBatch, m_MetricsFileStream);
            await WriteToDisk(m_TracesBatch, m_TracesFileStream);

            //dataCollector.ClearAll();
        }
    }

    private async Task WriteToDisk<T>(List<T> batch, FileStream fileStream) where T : ICsvSerializable
    {
        if (batch.Count == 0)
            return;

        m_WriteSignal.Wait();

        if (batch.Count == 0)
            return;

        fileStream.Seek(0, SeekOrigin.End);
        await using var writer = new StreamWriter(fileStream, Encoding.UTF8, leaveOpen: true);

        foreach (var item in batch)
            await writer.WriteLineAsync(item!.ToCsv());

        await writer.FlushAsync();
        batch.Clear();
        m_WriteSignal.Set();
    }

    private void TelemetryData_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is not NotifyCollectionChangedAction.Add)
            return;

        // Note: this usually gives back an optimized SingleItemList thingy
        foreach (var item in e.NewItems!)
        {
            ProcessItem(item);
        }
    }

    private void ProcessItem(object item)
    {
        switch (item)
        {
            case LogRecord log:
                m_LogsBatch.Add(new LogEntry()
                {
                });
                break;
            case Metric metric:
                foreach (ref readonly var point in metric.GetMetricPoints())
                {
                    double value;
                    switch (metric.MetricType)
                    {
                        case MetricType.LongGauge:
                        case MetricType.LongSum:
                            value = point.GetSumLong();
                            break;
                        case MetricType.DoubleGauge:
                        case MetricType.DoubleSum:
                            value = point.GetSumDouble();
                            break;
                        case MetricType.Histogram:
                            value = point.GetHistogramSum();
                            break;
                        case MetricType.ExponentialHistogram:
                        case MetricType.LongSumNonMonotonic:
                        case MetricType.DoubleSumNonMonotonic:
                        default:
                            continue;
                    }

                    var entry = new MetricEntry()
                    {
                        Name = metric.Name,
                        Description = metric.Description,
                        MeterName = metric.MeterName,
                        Type = metric.MetricType.ToString(),
                        Unit = metric.Unit
                    };
                }

                m_MetricsBatch.Add(new MetricsEntry()
                {
                });
                break;
            case Activity trace:
                m_TracesBatch.Add(new TraceEntry()
                {
                });
                break;
        }
    }
}