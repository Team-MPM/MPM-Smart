using Microsoft.AspNetCore.Mvc;
using PluginBase.Services.Devices;

namespace Backend.Endpoints;

public static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/devices", GetAllDevices);
        endpoints.MapGet("/api/devices/scan", ScanDevices);
        return endpoints;
    }

    public static IResult GetAllDevices([FromServices] DeviceManager deviceManager) =>
        Results.Json(deviceManager.ConnectedDevices);

    public static IResult ScanDevices([FromServices] DeviceTypeRegistry deviceTypeRegistry) =>
        Results.Ok(deviceTypeRegistry.ScanDevices());
}