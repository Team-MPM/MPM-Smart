namespace PluginBase.Services.Devices;

public class Device
{
    public IDeviceType Type { get; set; }
    public DeviceInfo DeviceInfo { get; set; }
    public DeviceMeta MetaData { get; set; }
    public Dictionary<string, string> ConnectionDetails { get; set; }
}