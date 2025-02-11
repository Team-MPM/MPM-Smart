using System.Net.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Data;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;
using PluginBase.Services.Permissions;
using Shared;
using SmartDevicePlugin;

namespace EnvironmentSensorPlugin;

public class EnvironmentSensorPluginClass : PluginBase<EnvironmentSensorPluginClass>
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
        services.AddSingleton<EnvironmentSensorController>();
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        var deviceRegistry = Services!.GetRequiredService<DeviceRegistry>();
        var controller = Services!.GetRequiredService<EnvironmentSensorController>();
        var dataIndex = Services!.GetRequiredService<DataIndex>();
        var permissionProvider = ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        var deviceTypeRegistry = ApplicationServices.GetRequiredService<DeviceTypeRegistry>();
        var deviceType = deviceTypeRegistry.GetDeviceType<SmartDeviceType>();
        
        permissionProvider.AddRange("EnvironmentSensor", ["EnvironmentSensor.Read"]);
                
        deviceRegistry.DeviceRegistered += device =>
        {
            if (device.Info.Type is SmartDeviceType smartDeviceType)
            {
                if (device.Info.Capabilities.ContainsKey("environment_sensor"))
                    controller.RegisterDevice(device);
            }
        };
        
        _ = Task.Run(async () =>
        {
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
        
        dataIndex.Add(new DataPoint
        {
            Name = "Temperature",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "°C",
            Plugin = this,
            ComboOptions = ["573412826234"],
            Permission = "EnvironmentSensor.Temp.Read",
            QueryHandler = async query => await controller.ProcessTempQuery(query)
        });
        
        dataIndex.Add(new DataPoint
        {
            Name = "Humidity",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "%",
            Plugin = this,
            ComboOptions = ["573412826234"],
            Permission = "EnvironmentSensor.Hum.Read",
            QueryHandler = async query => await controller.ProcessHumidityQuery(query)
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