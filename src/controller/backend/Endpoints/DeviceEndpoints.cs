using ApiSchema.Devices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PluginBase.Services.Devices;

namespace Backend.Endpoints;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/devices", GetAllDevices);
        endpoints.MapGet("/api/device/{serial}", GetDeviceBySerial);
        endpoints.MapGet("/api/sensors", GetAllSensors);
        endpoints.MapGet("/api/devices/scan", ScanDevices);
        endpoints.MapHub<DeviceHub>("/hubs/devices");
        return endpoints;
    }

    private static IResult GetDeviceBySerial([FromRoute] string serial, [FromServices] DeviceRegistry registry) =>
        registry.Devices.FirstOrDefault(d => d.Info.Serial == serial) is not Device device
            ? Results.NotFound()
            : Results.Json(device.MapToDto());

    private static IResult GetAllSensors([FromServices] DeviceRegistry registry) =>
        Results.Json(registry.Devices
            .Where(d => d.Info.Type.IsSensor)
            .Select(d => new SensorDto(d.Info.Name, d.Info.Serial, d.Info.Type.Plugin.RegistryName)));

    public static IResult GetAllDevices([FromServices] DeviceRegistry registry) =>
        Results.Json(registry.Devices.Select(i => i.MapToDto()));

    public static IResult ScanDevices([FromServices] DeviceTypeRegistry deviceTypeRegistry) =>
        Results.Ok(deviceTypeRegistry.ScanDevices().ToBlockingEnumerable().Select(i => i.MapToDto()));

    public static DeviceInfoDto MapToDto(this DeviceInfo info)
        => new(
            Name: info.Name,
            Description: info.Description,
            Serial: info.Serial,
            Type: info.Type.GetType().Name,
            Parameters: info.Type.Parameters,
            Details: info.Details,
            Capabilities: info.Capabilities
        );

    public static DeviceDto MapToDto(this Device device)
        => new(
            device.Info.MapToDto(),
            device.State,
            device.MetaData.Location
        );
}

public class DeviceHub(DeviceTypeRegistry deviceTypeRegistry, ILogger<DeviceHub> logger) : Hub
{
    [HubMethodName("Scan")]
    public async Task ScanDevices()
    {
        await foreach (var device in deviceTypeRegistry.ScanDevices())
        {
            await Clients.Caller.SendAsync("DeviceFound", device.MapToDto());
        }

        await Clients.Caller.SendAsync("DeviceScanFinished");
    }

    [HubMethodName("TryConnect")]
    public async Task TryConnect(DeviceInfoDto deviceInfoDto, IDictionary<string, object> parameters, string location)
    {
        logger.LogInformation("Attempting to connect to device {DeviceName}", deviceInfoDto.Name);
        
        var deviceType = deviceTypeRegistry.GetRegisteredDevices()
            .FirstOrDefault(t => t.GetType().Name == deviceInfoDto.Type);

        if (deviceType is null)
        {
            await Clients.Caller.SendAsync("ConnectionFailed");
            return;
        }

        var deviceInfo = new DeviceInfo
        {
            Name = deviceInfoDto.Name,
            Description = deviceInfoDto.Description,
            Serial = deviceInfoDto.Serial,
            Type = deviceType,
            Capabilities = deviceInfoDto.Capabilities,
            Details = deviceInfoDto.Details
        };

        var device = await deviceType.ConnectAsync(deviceInfo, new DeviceMeta { Location = location }, parameters);

        if (device is null)
        {
            await Clients.Caller.SendAsync("ConnectionFailed");
            return;
        }

        await Clients.Caller.SendAsync("ConnectionSucceeded", device.MapToDto());
    }
}