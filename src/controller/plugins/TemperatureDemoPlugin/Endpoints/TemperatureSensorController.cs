using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shared.Services.Sensors.TempDemo;
using TemperatureDemoPlugin.Data;

namespace TemperatureDemoPlugin.Endpoints;

public class TemperatureSensorController(TemperatureDemoContext dbContext)
{

    // ------------------ Sensors ------------------
    public async Task<IResult> GetSensors()
    {
        return Results.Ok(await dbContext.Sensors.Select(s => new {s.Id, s.Name, s.LastUpdateDate, s.IpAddress}).ToListAsync());
    }

    public async Task<IResult> GetSensor(int id)
    {
        var sensor = await dbContext.Sensors.FirstOrDefaultAsync(s => s.Id == id);
        if(sensor is null)
            return Results.NotFound("Sensor not found");
        return Results.Ok(new
        {
            Id = sensor.Id,
            Name = sensor.Name,
            LastUpdateDate = sensor.LastUpdateDate,
            IpAddress = sensor.IpAddress
        });
    }

    public async Task<IResult> AddSensor(HttpContext context)
    {
        var sensor = new Sensor()
        {
            Name = "Temperature Sensor" + await dbContext.Sensors.CountAsync() + 1,
            Token = Guid.NewGuid().ToString(),
            LastUpdateDate = DateTime.Now,
            IpAddress = context.Connection.RemoteIpAddress!.ToString()
        };
        dbContext.Sensors.Add(sensor);
        await dbContext.SaveChangesAsync();
        return Results.Created($"/api/sensor/{sensor.Id}", sensor);
    }


    // ------------------ SensorData ------------------

    public async Task<IResult> GetSensorData()
    {
        var dataEntries = await dbContext.DataEntries.Include(s => s.Sensor).ToListAsync();
        var groupedEntries = dataEntries.GroupBy(s => s.Sensor);
        Dictionary<int, List<DataEntry>> entries = new Dictionary<int, List<DataEntry>>();
        foreach (var group in groupedEntries)
            entries.Add(group.Key.Id, group.ToList());
        var result = entries.Select(s => new {Sensor = s.Key, Data = s.Value.Select(c => new {c.Id, c.CaptureTime, c.HumidityPercent, c.TemperatureC})});
        return Results.Ok(result);
    }
    public async Task<IResult> GetSensorData(int id, int span, string type)
    {
        // THIS DOES NOT WORK!
        var sensor = await dbContext.Sensors.FirstOrDefaultAsync(s => s.Id == id);
        if (sensor is null)
            return Results.NotFound("Sensor not found");
        TimeRange range = type switch
        {
            "any" => TimeRange.Any,
            "second" => TimeRange.Second,
            "minute" => TimeRange.Minute,
            "hour" => TimeRange.Hour,
            "day" => TimeRange.Day,
            "month" => TimeRange.Month,
            _ => TimeRange.Hour
        };
        List<DataEntry> result = new();
        switch (range)
        {
            case TimeRange.Any:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id).ToListAsync();
                break;
            case TimeRange.Second:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id && d.CaptureTime > DateTime.Now.AddSeconds(-span)).ToListAsync();
                break;
            case TimeRange.Minute:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id && d.CaptureTime > DateTime.Now.AddMinutes(-span)).ToListAsync();
                break;
            case TimeRange.Hour:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id && d.CaptureTime > DateTime.Now.AddHours(-span)).ToListAsync();
                break;
            case TimeRange.Day:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id && d.CaptureTime > DateTime.Now.AddDays(-span)).ToListAsync();
                break;
            case TimeRange.Month:
                result = await dbContext.DataEntries.Where(d => d.SensorId == sensor.Id && d.CaptureTime > DateTime.Now.AddMonths(-span)).ToListAsync();
                break;

        }
        var entries = result.Select(s => new {s.Id, s.CaptureTime, s.HumidityPercent, s.TemperatureC});
        return Results.Ok(entries);
    }

    public async Task<IResult> AddSensorEntry(HttpContext context, AddDemoTempEntry model)
    {
        var sensor = await dbContext.Sensors.FirstOrDefaultAsync(s => s.Token == model.Token);
        if (sensor is null)
            return Results.UnprocessableEntity("Token not assigned to a Sensor");
        if(context.Connection.RemoteIpAddress is not null)
            sensor.IpAddress = context.Connection.RemoteIpAddress.ToString();
        var entry = new DataEntry()
        {
            CaptureTime = DateTime.Now,
            HumidityPercent = model.Humidity,
            TemperatureC = model.Temperature,
            Sensor = sensor,
        };
        dbContext.DataEntries.Add(entry);
        sensor.LastUpdateDate = DateTime.Now;
        await dbContext.SaveChangesAsync();
        return Results.Created("/api/sensorentry", new {entry.Id, entry.CaptureTime, entry.HumidityPercent, entry.TemperatureC});

    }
}