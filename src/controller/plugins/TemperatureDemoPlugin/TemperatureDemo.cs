using System.Reflection;
using ApiSchema.Sensors.DemoTempSensor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase;
using PluginBase.Options;
using PluginBase.Services.Permissions;
using Shared.Plugins;
using Shared.Plugins.DataInfo;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;
using Shared.Services.Sensors.TempDemo;
using TemperatureDemoPlugin.Data;
using TemperatureDemoPlugin.Endpoints;
using TemperatureDemoPlugin.Permissions;
using TemperatureDemoPlugin.Services;
using ILogger = Serilog.ILogger;

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

        group.MapDelete("/sensors/{id}", async (
            [FromRoute] int id) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.DeleteSensor(id);
        }).RequirePermission(TemperatureClaims.DeleteSensors);


        group.MapPost("/update", async (
            HttpContext context,
            UpdateSensorData model) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.UpdateSensorData(context, model);
        });

        group.MapPost("/regenerateToken", async (
            HttpContext context,
            RegenerateTokenModel model) =>
        {
            var controller = Services!.GetRequiredService<TemperatureSensorController>();
            return await controller.GenerateNewToken(context, model);
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

        group.MapGet("/sensorentries", async () =>
        {
            var dbContext = Services!.GetRequiredService<TemperatureDemoContext>();
            var logger = Services!.GetRequiredService<ILogger<TemperatureDemo>>();
            dbContext.DataEntries.Add(new()
            {
                Sensor = dbContext.Sensors.First(),
                TemperatureC = 20,
                HumidityPercent = 50,
                CaptureTime = DateTime.Now
            });
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Added entry!");
        });
    }

    public override async Task<DataInfoPluginResponse> GetPluginDataInfo()
    {
        var result = new DataInfoPluginResponse()
        {
            IsSuccessful = true,
            SensorEntries = new List<DataInfoSensorEntry>()
        };
        var dbContext = Services!.GetRequiredService<TemperatureDemoContext>();
        var sensors = await dbContext.Sensors.ToListAsync();
        foreach (var sensor in sensors)
        {
            result.SensorEntries.Add(new()
            {
                SensorName = sensor.Name,
                RequestableDataTypes = new List<string>() {"TemperatureC", "HumidityPercent"}
            });
        }
        return result;
    }

    public override async Task<DataResponseInfo> GetDataFromPlugin(DataRequestEntry request)
    {
        var dbContext = Services!.GetRequiredService<TemperatureDemoContext>();
        Type? dataType = Type.GetType("string");
        var data = await dbContext.DataEntries
            .Include(s => s.Sensor)
            .Where(s => s.Sensor.Name == request.SensorName)
            .Where(s => s.CaptureTime > request.StartDate && s.CaptureTime < request.EndDate)
            .ToListAsync();

        var propertyValues = data
            .Select(entry =>
            {
                var entryType = entry.GetType();

                var propertyInfo = entryType.GetProperty(request.RequestedDataType);
                if (propertyInfo == null)
                {
                    throw new InvalidOperationException($"Property '{request.RequestedDataType}' not found on type '{entryType.Name}'.");
                }

                dataType = propertyInfo.PropertyType;

                var propertyValue = propertyInfo.GetValue(entry);

                return new PropertyValue()
                {
                    Data = propertyValue,
                    CaptureDate = entry.CaptureTime
                };
            })
            .ToList();
        return new DataResponseInfo
        {
            IsSuccessful = true,
            PluginName = Name,
            SensorName = request.SensorName,
            DataName = request.RequestedDataType,
            DataType = dataType!.ToString(),
            Data = propertyValues.Select(s => new DataResponseEntry
            {
                Data = s.Data!,
                CaptureDate = s.CaptureDate
            }).ToList()
        };
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

    protected override void OnOptionBuilding(OptionsBuilder builder)
    {

    }
}

public record PropertyValue
{
    public object? Data { get; init; }
    public DateTime CaptureDate { get; init; }
}