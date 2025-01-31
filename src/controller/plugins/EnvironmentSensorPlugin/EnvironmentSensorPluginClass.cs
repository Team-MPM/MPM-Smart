using System.Net.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;
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
        var clientFactory = Services!.GetRequiredService<IHttpClientFactory>();
        
        deviceRegistry.DeviceRegistered += device =>
        {
            if (device.Info.Type is SmartDeviceType smartDeviceType)
            {
                
            }
        };
        
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        return Task.CompletedTask;
    }
}