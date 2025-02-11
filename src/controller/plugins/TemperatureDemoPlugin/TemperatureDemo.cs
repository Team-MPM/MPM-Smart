using LanguageExt.ClassInstances.Pred;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using PluginBase.Services.Data;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;
using PluginBase.Services.Permissions;
using Shared;
using TemperatureDemoPlugin.Data;
using TemperatureDemoPlugin.Permissions;

namespace TemperatureDemoPlugin;

public class TemperatureDemo : PluginBase<TemperatureDemo>
{
    protected override Task Initialize()
    {
        return Task.CompletedTask;
    }

    protected override Task BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        return Task.CompletedTask;
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TemperatureDemoContext>(options =>
        {
            options.UseSqlite("Data Source=TemperatureDemo.db");
            options.EnableDetailedErrors();
        });
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        var deviceTypeRegistry = ApplicationServices.GetRequiredService<DeviceTypeRegistry>();
        deviceTypeRegistry.RegisterDeviceType(new DemoTempDeviceType { Plugin = this });
        var deviceType = deviceTypeRegistry.GetDeviceType<DemoTempDeviceType>();
        var logger = Services!.GetRequiredService<ILogger<TemperatureDemo>>();
        var deviceRegistry = Services!.GetRequiredService<DeviceRegistry>();
        logger.LogInformation("Starting Temp Demo Plugin");
        _ = Task.Run(async () =>
        {
            await deviceRegistry.RegisterDeviceAsync(new Device()
            {
                Info = new DeviceInfo
                {
                    Name = "Temperature Sensor Demo 1",
                    Description = "Temperature Sensor Demo 1",
                    Serial = "123412341234",
                    Type = deviceType!,
                    Capabilities = new Dictionary<string, string>()
                    {
                        { "temperature-demo", "temperature-demo-plugin" }
                    },
                    Details = new Dictionary<string, string>()
                },
                State = DeviceState.Connected,
                MetaData = new DeviceMeta()
                {
                    Location = "Living Room",
                    ConnectionDetails = new Dictionary<string, string>()
                }
            });
            await deviceRegistry.RegisterDeviceAsync(new Device()
            {
                Info = new DeviceInfo
                {
                    Name = "Environment Sensor 1",
                    Description = "Environment Sensor 1",
                    Serial = "573412826234",
                    Type = deviceType!,
                    Capabilities = new Dictionary<string, string>()
                    {
                        { "environment_sensor", "environment_sensor" }
                    },
                    Details = new Dictionary<string, string>()
                },
                State = DeviceState.Connected,
                MetaData = new DeviceMeta()
                {
                    Location = "Living Room",
                    ConnectionDetails = new Dictionary<string, string>()
                }
            });
            
        });
        

        var permissionProvider = ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        permissionProvider.AddRange("TemperatureDemo", TemperatureClaims.ExportPermissions());

        var index = ApplicationServices.GetRequiredService<DataIndex>();
        index.Add(new DataPoint
        {
            Name = "Temperature Singe",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "°C",
            Plugin = this,
            ComboOptions = ["123412341234", "Kitchen", "Living Room", "Bed Room"],
            Permission = TemperatureClaims.ViewSensorData,
            QueryHandler = query => Task.FromResult<DataQueryResult?>(new ComboQueryResult(new Dictionary<string, object>
            {
                ["123412341234"] = Random.Shared.NextDouble() * 20,
                ["Kitchen"] = Random.Shared.NextDouble() * 20,
                ["Living Room"] = Random.Shared.NextDouble() * 20,
                ["Bed Room"] = Random.Shared.NextDouble() * 20
            }))
        });

        index.Add(new DataPoint
        {
            Name = "Temperature",
            Description = "Temperature",
            QueryType = DataQueryType.ComboSeriesDouble,
            Unit = "°C",
            Plugin = this,
            AvailableGranularity =
                [TimeSpan.FromDays(1), TimeSpan.FromHours(4), TimeSpan.FromHours(1), TimeSpan.FromMinutes(10)],
            ComboOptions = ["123412341234", "Kitchen", "Living Room", "Bed Room"],
            Permission = TemperatureClaims.ViewSensorData,
            QueryHandler = query => Task.FromResult<DataQueryResult?>(new ComboSeriesQueryResult(new Dictionary<string, object[]>
            {
                ["123412341234"] = [Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20, Random.Shared.NextDouble() * 20],
                ["Kitchen"] = [20.0d, 21.0d, 22.0d, 23.0d],
                ["Living Room"] = [22.0d, 23.0d, 24.0d, 25.0d],
                ["Bed Room"] = [18.0d, 19.0d, 20.0d, 21.0d]
            }))
        });
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        return Task.CompletedTask;
    }
}

public class DemoTempDeviceType : IDeviceType
{
    public required IPlugin Plugin { get; init; }
    public IDictionary<string, string> Parameters => new Dictionary<string, string>();
    public bool IsSensor => true;

    public IAsyncEnumerable<DeviceInfo> ScanAsync()
    {
        return AsyncEnumerable.Empty<DeviceInfo>();
    }

    public Task<Device?> ConnectAsync(DeviceInfo deviceInfo, DeviceMeta metadata,
        IDictionary<string, object> parameters)
    {
        return Task.FromResult<Device?>(null);
    }

    public Task PollAsync(Device device)
    {
        return Task.CompletedTask;
    }
}