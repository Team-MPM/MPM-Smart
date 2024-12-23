namespace PluginBase.Services.Devices;

public interface IDeviceType
{
    public IPlugin Plugin { get; }

    public IAsyncEnumerable<DeviceInfo> ScanAsync(IServiceProvider services);
    public Task<bool> ConnectAsync(DeviceInfo deviceInfo, IDictionary<string, object> parameters);
}