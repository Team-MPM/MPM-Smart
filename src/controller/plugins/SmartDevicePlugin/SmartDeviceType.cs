using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Devices;
using PluginBase.Services.Networking;

namespace SmartDevicePlugin;

public record MpmSmartDeviceInfo(
    string Id,
    string Name,
    string Description,
    Dictionary<string, string> Capabilities,
    string Status,
    string Serial,
    string PublicKey);

public class SmartDeviceType : IDeviceType
{
    public IPlugin Plugin { get; init; }

    public static SmartDeviceType Instance { get; } = new();

    public async IAsyncEnumerable<DeviceInfo> ScanAsync(IServiceProvider services)
    {
        var scanner = services.GetRequiredService<NetworkScanner>();
        var devices = services.GetRequiredService<DeviceManager>();
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);

        await foreach (var ip in scanner.ScanTcpAsync(80))
        {
            if (devices.ConnectedDevices.Any(d =>
                {
                    d.Info.Info.TryGetValue("ip", out var existingIp);
                    return existingIp == ip;
                }))
            {
                continue;
            }

            var response = await client.GetAsync($"http://{ip}/info");

            MpmSmartDeviceInfo info;
            try
            {
                var res = await response.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();
                if (res is not { Status: "ready" })
                    continue;
                info = res;
            }
            catch (Exception)
            {
                continue;
            }

            yield return new DeviceInfo
            {
                Name = info.Name,
                Description = info.Description,
                Type = this,
                Serial = info.Serial,
                Parameters = new Dictionary<string, object>(),
                Capabilities = info.Capabilities,
                Info = new Dictionary<string, string>()
                {
                    { "ip", ip }
                }
            };
        }
    }

    public async Task<Device?> ConnectAsync(DeviceInfo deviceInfo, DeviceMeta metadata,
        IDictionary<string, object> parameters, IServiceProvider sp)
    {
        var client = new HttpClient();
        var deviceManager = sp.GetRequiredService<DeviceManager>();

        if (deviceManager.ConnectedDevices.Any(d =>
            {
                d.Info.Info.TryGetValue("ip", out var existingIp);
                return existingIp == deviceInfo.Info["ip"];
            }))
        {
            return null;
        }

        var infoResponse = await client.GetAsync($"http://{deviceInfo.Info["ip"]}/info");

        if (!infoResponse.IsSuccessStatusCode)
            return null;

        var info = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();

        var connectResponse = await client.GetAsync($"http://{deviceInfo.Info["ip"]}/connect");

        if (!connectResponse.IsSuccessStatusCode)
            return null;

        var key = await connectResponse.Content.ReadAsStringAsync();

        var connectAkkResponse = await client.GetAsync($"http://{deviceInfo.Info["ip"]}/connect-akk");

        if (!connectAkkResponse.IsSuccessStatusCode)
            return null;

        metadata.ConnectionDetails["key"] = key;

        return new Device
        {
            Info = deviceInfo,
            MetaData = metadata
        };
    }

    public async Task<bool> PollAsync(Device device)
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri($"http://{device.Info.Info["ip"]}");

        var infoResponse = await client.GetAsync("/info");

        if (!infoResponse.IsSuccessStatusCode)
            return false;

        var info = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();

        var response = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();

        if (response is not { Status: "connected" })
            return false;

        // TODO check if key is still valid
        var key = device.MetaData.ConnectionDetails.GetValueOrDefault("key");

        if (key is null)
            return false;

        return true;
    }

    public async Task ReconnectAsync(IServiceProvider sp)
    {
        // var devices = SmartDeviceIndex.Entries;
        // var deviceManager = sp.GetRequiredService<DeviceManager>();
        // var deviceTypeRegistry = sp.GetRequiredService<DeviceTypeRegistry>();
        // var client = new HttpClient();
        // client.Timeout = TimeSpan.FromSeconds(2);
        //
        // foreach (var device in devices.Where(d => deviceManager.ConnectedDevices.All(
        //              cd => cd.ConnectionDetails.GetValueOrDefault("ip") != d.Ip
        //                    || cd.Type.GetType() != typeof(SmartDeviceType))))
        // {
        //     client.BaseAddress = new Uri($"http://{device.Ip}");
        //     var infoResponse = await client.GetAsync("/info");
        //
        //     if (!infoResponse.IsSuccessStatusCode)
        //         continue;
        //
        //     var info = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();
        //
        //     if (info is null)
        //         continue;
        //
        //     deviceManager.AddConnectedDevice(new Device()
        //     {
        //         ConnectionDetails = { { "key", device.Key }, { "ip", device.Ip } },
        //         Type = this,
        //         Info = new DeviceInfo
        //         {
        //             Name = info.Name,
        //             Description = info.Description,
        //             Type = this,
        //             Parameters = new Dictionary<string, object>(),
        //             Capabilities = info.Capabilities,
        //             Info = new Dictionary<string, string>()
        //             {
        //                 { "ip", device.Ip }
        //             }
        //         },
        //         MetaData = {}
        //     });
        // }
    }
}