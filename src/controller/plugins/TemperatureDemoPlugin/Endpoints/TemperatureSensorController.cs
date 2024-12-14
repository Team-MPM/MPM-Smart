using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Services.Sensors.TempDemo;
using TemperatureDemoPlugin.Data;

namespace TemperatureDemoPlugin.Endpoints;

public class TemperatureSensorController(TemperatureDemoContext dbContext)
{
    public async Task<List<Sensor>> GetSensors()
    {
        return await dbContext.Sensors.Select(s => s).ToListAsync();
    }

    public async Task<IResult> AddSensorEntry(HttpContext context, AddDemoTempEntry model)
    {
        var sensor = await dbContext.Sensors.FirstOrDefaultAsync(s => s.Token == model.Token);
        if (sensor is null)
            return Results.UnprocessableEntity("Token not assigned to a Sensor");
        if(context.Connection.RemoteIpAddress is not null)
            sensor.IpAddress = context.Connection.RemoteIpAddress.ToString();
        dbContext.DataEntries.Add(new()
        {
            CaptureTime = DateTime.Now,
            HumidityPercent = model.Humidity,
            TemperatureC = model.Temperature,
            Sensor = sensor,
        });
        sensor.LastUpdateDate = DateTime.Now;
        await dbContext.SaveChangesAsync();
        return Results.Created();

    }
}