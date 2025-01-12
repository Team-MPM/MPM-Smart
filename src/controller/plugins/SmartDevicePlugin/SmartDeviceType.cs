using System.Net.Http.Json;
using PluginBase;
using PluginBase.Services;
using PluginBase.Services.Devices;
using PluginBase.Services.Networking;
using Shared;

namespace SmartDevicePlugin;

// ReSharper disable once ClassNeverInstantiated.Global
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
    public required IPlugin Plugin { get; init; }
    public IDictionary<string, string> Parameters => new Dictionary<string, string>();
    public bool IsSensor => true;

    private readonly DeviceRegistry m_DeviceRegistry = ServiceProviderHelper.GetService<DeviceRegistry>();
    private readonly NetworkScanner m_NetworkScanner = ServiceProviderHelper.GetService<NetworkScanner>();
    private readonly IHttpClientFactory m_HttpClientFactory = ServiceProviderHelper.GetService<IHttpClientFactory>();

    public async IAsyncEnumerable<DeviceInfo> ScanAsync()
    {
        var client = m_HttpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(2);

        await foreach (var ip in m_NetworkScanner.ScanTcpAsync(80))
        {
            if (m_DeviceRegistry.Devices.Any(d =>
                {
                    d.Info.Details.TryGetValue("ip", out var existingIp);
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
                Capabilities = info.Capabilities,
                Details = new Dictionary<string, string>()
                {
                    { "ip", ip }
                }
            };
        }
    }

    public async Task<Device?> ConnectAsync(DeviceInfo deviceInfo, DeviceMeta metadata,
        IDictionary<string, object> parameters)
    {
        var client = m_HttpClientFactory.CreateClient();

        if (m_DeviceRegistry.Devices.Any(d =>
            {
                d.Info.Details.TryGetValue("ip", out var existingIp);
                return existingIp == deviceInfo.Details["ip"];
            }))
        {
            return null;
        }

        var infoResponse = await client.GetAsync($"http://{deviceInfo.Details["ip"]}/info");

        if (!infoResponse.IsSuccessStatusCode)
            return null;

        var info = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();

        var connectResponse = await client.GetAsync($"http://{deviceInfo.Details["ip"]}/connect");

        if (!connectResponse.IsSuccessStatusCode)
            return null;

        var key = await connectResponse.Content.ReadAsStringAsync();

        var connectAkkResponse = await client.GetAsync($"http://{deviceInfo.Details["ip"]}/connect-akk");

        if (!connectAkkResponse.IsSuccessStatusCode)
            return null;

        metadata.ConnectionDetails["key"] = key;

        return new Device
        {
            Info = deviceInfo,
            State = DeviceState.Connected,
            MetaData = metadata
        };
    }

    public async Task PollAsync(Device device)
    {
        var client = m_HttpClientFactory.CreateClient();
        client.BaseAddress = new Uri($"http://{device.Info.Details["ip"]}");

        try
        {
            var infoResponse = await client.GetAsync("/info");
            if (!infoResponse.IsSuccessStatusCode)
                goto disconnect;

            var info = await infoResponse.Content.ReadFromJsonAsync<MpmSmartDeviceInfo>();

            if (info is not { Status: "connected" })
                goto disconnect;

            // TODO check if key is still valid
            var key = device.MetaData.ConnectionDetails.GetValueOrDefault("key");

            if (key is null)
                goto authFailure;
        }
        catch (Exception)
        {
            goto disconnect;
        }

        device.State = DeviceState.Connected;
        return;

        disconnect:
        device.State = DeviceState.Disconnected;
        return;

        authFailure:
        device.State = DeviceState.Unauthorized;
    }
}