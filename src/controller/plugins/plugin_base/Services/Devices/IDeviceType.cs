using System.Text.Json.Serialization;

namespace PluginBase.Services.Devices;

[JsonConverter(typeof(DeviceTypeJsonConverter))]
public interface IDeviceType
{
    public IPlugin Plugin { get; }

    public IAsyncEnumerable<DeviceInfo> ScanAsync(IServiceProvider services);

    public Task<Device?> ConnectAsync(DeviceInfo deviceInfo, DeviceMeta metadata,
        IDictionary<string, object> parameters, IServiceProvider sp);

    public Task<bool> PollAsync(Device device);
    public Task ReconnectAsync(IServiceProvider sp);
}