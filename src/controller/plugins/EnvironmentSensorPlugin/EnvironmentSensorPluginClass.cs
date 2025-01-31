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
        
        permissionProvider.AddRange("EnvironmentSensor", ["EnvironmentSensor.Read"]);
        
        deviceRegistry.DeviceRegistered += device =>
        {
            if (device.Info.Type is SmartDeviceType smartDeviceType)
            {
                //if (device.Info.Capabilities.ContainsKey("environment_sensor"))
                    controller.RegisterDevice(device);
            }
        };
        
        dataIndex.Add(new DataPoint
        {
            Name = "Temperature",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "°C",
            Plugin = this,
            ComboOptions = ["Sensor 1"],
            Permission = "EnvironmentSensor.Temp.Read",
            QueryHandler = async query => await controller.ProcessTempQuery(query)
        });
        
        dataIndex.Add(new DataPoint
        {
            Name = "Humidity",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "°C",
            Plugin = this,
            ComboOptions = ["Sensor 1"],
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