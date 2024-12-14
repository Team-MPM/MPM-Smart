using Backend.Services.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using Serilog;
using Shared.Services.Permissions;
using Shared.Services.Sensors.TempDemo;
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

        group.MapGet("/sensors", async () =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return controller.GetSensors();
        });

        group.MapPost("/sensorentry", async (
            HttpContext context,
            [FromBody] AddDemoTempEntry model) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.AddSensorEntry(context, model);
        });
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TemperatureDemoContext>(options =>
        {
            options.UseSqlite("Data Source=TemperatureDemo.db");
            options.EnableDetailedErrors();
        });
        services.AddHostedService<TemperatureDataFiller>();
        services.AddScoped<TemperatureSensorController>();
    }

    protected override void SystemStart()
    {
        var permissionProvider = ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        permissionProvider.AddRange("TemperatureDemo", TemperatureClaims.ExportPermissions());

    }

}