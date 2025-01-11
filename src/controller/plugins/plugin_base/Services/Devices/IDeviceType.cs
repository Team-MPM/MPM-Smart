using System.Text.Json.Serialization;

namespace PluginBase.Services.Devices;

[JsonConverter(typeof(DeviceTypeJsonConverter))]
public interface IDeviceType
{
    public IPlugin Plugin { get; }

    public IDictionary<string, string> Parameters { get; }
    public bool IsSensor { get; }

    public IAsyncEnumerable<DeviceInfo> ScanAsync();

    public Task<Device?> ConnectAsync(DeviceInfo deviceInfo, DeviceMeta metadata,
        IDictionary<string, object> parameters);

    public Task PollAsync(Device device);
}