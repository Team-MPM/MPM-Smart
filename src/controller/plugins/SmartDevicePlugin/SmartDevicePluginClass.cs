using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;

namespace SmartDevicePlugin;

public class SmartDevicePluginClass : PluginBase<SmartDevicePluginClass>
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
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        var logger = Services!.GetRequiredService<ILogger<SmartDevicePluginClass>>();
        var deviceRegistry = Services!.GetRequiredService<DeviceTypeRegistry>();
        logger.LogInformation("Starting Smart Device Plugin");
        deviceRegistry.RegisterDeviceType(new SmartDeviceType { Plugin = this });
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        return Task.CompletedTask;
    }
}