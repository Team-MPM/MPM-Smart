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
        endpoints.MapGet("/api/devices/scan", ScanDevices);
        endpoints.MapHub<DeviceHub>("/hubs/devices");
        return endpoints;
    }

    public static IResult GetAllDevices([FromServices] DeviceManager deviceManager) =>
        Results.Json(deviceManager.ConnectedDevices.Select(i => i.MapToDto()));

    public static IResult ScanDevices([FromServices] DeviceTypeRegistry deviceTypeRegistry) =>
        Results.Ok(deviceTypeRegistry.ScanDevices().ToBlockingEnumerable().Select(i => i.MapToDto()));

    public static DeviceInfoDto MapToDto(this DeviceInfo into)
        => new(
            Name: into.Name,
            Description: into.Description,
            Capabilities: into.Capabilities
        );

    public static DeviceDto MapToDto(this Device device)
        => new(
            device.Info.MapToDto(),
            device.State,
            device.MetaData.Location
        );
}

public class DeviceHub(DeviceTypeRegistry deviceTypeRegistry) : Hub
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
}