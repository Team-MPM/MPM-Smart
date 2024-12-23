using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Devices;
using PluginBase.Services.Networking;

namespace SmartDevicePlugin;

public record MpmSmartDeviceInfo(string Name, string Description, Dictionary<string, string> Capabilities);

public class SmartDeviceType : IDeviceType
{
    public required IPlugin Plugin { get; init; }

    public async IAsyncEnumerable<DeviceInfo> ScanAsync(IServiceProvider services)
    {
        var scanner = services.GetRequiredService<NetworkScanner>();
        var devices = services.GetRequiredService<DeviceManager>();
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(2);

        await foreach(var ip in scanner.ScanTcpAsync(80))
        {
            if (devices.ConnectedDevices.Any(d =>
                {
                    d.Info.TryGetValue("ip", out var existingIp);
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
                if (res == null)
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
                Parameters = new Dictionary<string, object>(),
                Capabilities = info.Capabilities,
                Info = new Dictionary<string, string>()
                {
                    { "ip", ip }
                }
            };
        }
    }

    public Task<bool> ConnectAsync(DeviceInfo deviceInfo, IDictionary<string, object> parameters)
    {
        throw new NotImplementedException();
    }
}