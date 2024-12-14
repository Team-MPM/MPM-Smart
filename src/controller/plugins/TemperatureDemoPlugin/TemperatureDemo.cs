using Backend.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
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
            return await controller.GetSensors();
        });

        group.MapGet("/sensors/{id}", async (
        [FromRoute] int id) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.GetSensor(id);
        });


        group.MapPost("/sensors", async (
            HttpContext context) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.AddSensor(context);
        });

        group.MapGet("/sensorentry", async () =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.GetSensorData();
        }).RequirePermission(TemperatureClaims.ViewSensorData);

        group.MapGet("/sensorentry/{id}", async (
            [FromRoute] int id,
            [FromQuery] int span = -1,
            [FromQuery] string type = "any") =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.GetSensorData(id, span, type);
        }).RequirePermission(TemperatureClaims.ViewSensorData);

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