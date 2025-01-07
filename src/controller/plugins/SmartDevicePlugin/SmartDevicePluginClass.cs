using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;

namespace SmartDevicePlugin;

public class SmartDevicePluginClass : PluginBase<SmartDevicePluginClass>
{
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
    }

    protected override void SystemStart()
    {
        var logger = Services!.GetRequiredService<ILogger<SmartDevicePluginClass>>();
        var deviceRegistry = Services!.GetRequiredService<DeviceTypeRegistry>();
        SmartDeviceIndex.Load();
        logger.LogInformation("Starting Smart Device Plugin");
        deviceRegistry.RegisterDevice(new SmartDeviceType { Plugin = this });
    }

    protected override void OnOptionBuilding(OptionsBuilder builder)
    {
    }
}