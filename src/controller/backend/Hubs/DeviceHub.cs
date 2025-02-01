using ApiSchema;
using Backend.Endpoints;
using Microsoft.AspNetCore.SignalR;
using PluginBase.Services.Devices;
using PluginBase.Services.General;

namespace Backend.Hubs;

public class DeviceHub(DeviceTypeRegistry deviceTypeRegistry, ILogger<DeviceHub> logger) : HubBase
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