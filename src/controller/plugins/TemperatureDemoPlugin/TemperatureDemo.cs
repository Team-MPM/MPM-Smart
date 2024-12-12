using Backend.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using Serilog;
using Shared.Services.Permissions;
using TemperatureDemoPlugin.Data;
using TemperatureDemoPlugin.Endpoints;
using TemperatureDemoPlugin.Permissions;
using TemperatureDemoPlugin.Services;

namespace TemperatureDemoPlugin;

public class TemperatureDemo : PluginBase<TemperatureDemo>
{
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        var group = routeBuilder.MapGroup("/api/temperature");

        // group.MapGet("/sensors",  () => throw new NotImplementedException());
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TemperatureDemoContext>(options =>
        {
            options.UseSqlite("Data Source=TemperatureDemo.db");
            options.EnableDetailedErrors();
        });
        services.AddHostedService<TemperatureDataFiller>();
        services.AddSingleton<TemperatureSensorController>();
    }

    protected override void SystemStart()
    {
        var permissionProvider =  ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        var logger =  ApplicationServices.GetService<ILogger<TemperatureDemo>>();
        if (permissionProvider is not null)
            permissionProvider.AddRange("TemperatureDemo", TemperatureClaims.ExportPermissions());
        else
            if(logger is not null)
                logger.LogError("Permission provider not found");
    }

}