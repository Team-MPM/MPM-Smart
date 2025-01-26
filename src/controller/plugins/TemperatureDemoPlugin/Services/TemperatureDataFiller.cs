using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TemperatureDemoPlugin.Data;

namespace TemperatureDemoPlugin.Services;

public class TemperatureDataFiller(TemperatureDemoContext context, ILogger<TemperatureDataFiller> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
        // while (!await context.Database.CanConnectAsync(stoppingToken))
        // {
        //     await Task.Delay(1000, stoppingToken);
        // }
        // await Task.WhenAll(
        //     FillData(stoppingToken)
        //     );
    }

    private async Task FillData(CancellationToken stoppingToken)
    {
        if (!context.Sensors.Any())
        {
            logger.LogInformation("No sensors found. Adding demo sensors");
            context.Sensors.AddRange(
                new Sensor { Name = "Sensor 1", IpAddress = "", Token = "aaa", LastUpdateDate = DateTime.Now},
                new Sensor { Name = "Sensor 2", IpAddress = "", Token = "bbb", LastUpdateDate = DateTime.Now },
                new Sensor { Name = "Sensor 3", IpAddress = "", Token = "ccc", LastUpdateDate = DateTime.Now }
            );
            await context.SaveChangesAsync(stoppingToken);
        }
        var sensors = context.Sensors.ToList();
        while(!stoppingToken.IsCancellationRequested)
        {
            var sensor = sensors.OrderBy(s => Guid.NewGuid()).First();
            var temperature = new DataEntry()
            {
                SensorId = sensor.Id,
                Sensor = sensor,
                TemperatureC = new Random().Next(-20, 30),
                HumidityPercent = new Random().Next(0, 100),
                CaptureTime = DateTime.Now
            };
            sensor.LastUpdateDate = temperature.CaptureTime;
            context.Update(sensor);
            context.DataEntries.Add(temperature);
            await context.SaveChangesAsync(stoppingToken);
            logger.LogInformation($"Added new data entry for sensor {sensor.Id}");
            await Task.Delay(1000, stoppingToken);
        }
    }
}