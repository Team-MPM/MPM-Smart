using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Options;
using Shared.Plugins.DataInfo;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;
using TelemetryPlugin.Data;
using TelemetryPlugin.Services;

namespace TelemetryPlugin;

public class TelemetryPlugin : PluginBase<TelemetryPlugin>
{
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TelemetryDbContext>(options =>
        {
            options.UseSqlite("Data Source=telemetry.db");
            options.EnableDetailedErrors();
        });

        services.AddSingleton<TelemetryDataProvider>();
        services.AddSingleton<TelemetryDataProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<TelemetryDataProcessor>());
    }

    protected override void SystemStart()
    {
    }

    protected override void OnOptionBuilding(OptionsBuilder builder)
    {
    }

    public override Task<DataInfoPluginResponse> GetPluginDataInfo()
    {
        return Task.FromResult(new DataInfoPluginResponse
        {
            IsSuccessful = true,
            SensorEntries =
            [
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.collections.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.objects.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.allocations.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.committed_memory.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.heap.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.heap.fragmentation.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.gc.duration", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.jit.il_compiled.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.jit.methods_compiled.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.jit.compilation_time", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.monitor.lock_contention.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.thread_pool.threads.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.thread_pool.completed_items.count",
                    RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.thread_pool.queue.length", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.timer.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.assemblies.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "process.runtime.dotnet.exceptions.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry { DataPoint = "kestrel.active_connections", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "kestrel.queued_connections", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "http.server.active_requests", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "http.server.request.duration", RequestableDataTypes = ["float"] },
                new DataPointEntry
                {
                    DataPoint = "aspnetcore.routing.match_attempts", RequestableDataTypes = ["float"]
                },
                new DataPointEntry { DataPoint = "kestrel.connection.duration", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.gc.collections", RequestableDataTypes = ["float"] },
                new DataPointEntry
                {
                    DataPoint = "dotnet.process.memory.working_set", RequestableDataTypes = ["float"]
                },
                new DataPointEntry { DataPoint = "dotnet.gc.heap.total_allocated", RequestableDataTypes = ["float"] },
                new DataPointEntry
                {
                    DataPoint = "dotnet.gc.last_collection.memory.committed_size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "dotnet.gc.last_collection.heap.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "dotnet.gc.last_collection.heap.fragmentation.size", RequestableDataTypes = ["float"]
                },
                new DataPointEntry { DataPoint = "dotnet.gc.pause.time", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.jit.compiled_il.size", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.jit.compiled_methods", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.jit.compilation.time", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.monitor.lock_contentions", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.thread_pool.thread.count", RequestableDataTypes = ["float"] },
                new DataPointEntry
                {
                    DataPoint = "dotnet.thread_pool.work_item.count", RequestableDataTypes = ["float"]
                },
                new DataPointEntry { DataPoint = "dotnet.thread_pool.queue.length", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.timer.count", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.assembly.count", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.process.cpu.count", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.process.cpu.time", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "dotnet.exceptions", RequestableDataTypes = ["float"] },
                new DataPointEntry { DataPoint = "kestrel.upgraded_connections", RequestableDataTypes = ["float"] },
                new DataPointEntry
                {
                    DataPoint = "signalr.server.active_connections", RequestableDataTypes = ["float"]
                },
                new DataPointEntry
                {
                    DataPoint = "signalr.server.connection.duration", RequestableDataTypes = ["float"]
                }
            ]
        });
    }

    public override Task RequestDataFromSensors()
    {
        return base.RequestDataFromSensors();
    }

    public override async Task<DataResponseInfo> GetDataFromPlugin(DataRequestEntry request)
    {
        using var scope = Services!.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        var metric = await db.Metrics.FirstOrDefaultAsync(m => m.Name == request.DataPoint);

        if (metric is null)
            return new DataResponseInfo
            {
                IsSuccessful = false,
                ErrorMessage = "The requested metric does not exist.",
                PluginName = RegistryName,
                DataPoint = request.DataPoint,
                DataName = request.DataPoint,
                DataType = "float"
            };

        if (metric.Type != MetricType.Counter)
            return new DataResponseInfo
            {
                IsSuccessful = false,
                ErrorMessage = "The requested metric is not a counter.",
                PluginName = RegistryName,
                DataPoint = request.DataPoint,
                DataName = request.DataPoint,
                DataType = "float"
            };

        var data = await db.CounterMetricEntries
            .Where(e => e.TimeStampStartUtc >= request.StartDate && e.TimeStampEndUtc <= request.EndDate)
            .OrderByDescending(e => e.TimeStampEndUtc)
            .Select(e => new DataResponseEntry()
            {
                Data = (float)e.Value,
                TimeStampUtc = e.TimeStampEndUtc
            })
            .ToListAsync();

        return new DataResponseInfo
        {
            IsSuccessful = true,
            ErrorMessage = null,
            PluginName = RegistryName,
            DataPoint = request.DataPoint,
            DataName = request.DataPoint,
            DataType = "float",
            Data = data
        };
    }
}