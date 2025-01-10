namespace PluginBase.Services.Devices;

public record DeviceInfo
{
    public required string Name { get; init; }
    public required string Serial { get; set; }
    public required string Description { get; init; }
    public required IDeviceType Type { get; init; }
    public required IDictionary<string, object> Parameters { get; init; }
    public required IDictionary<string, string> Capabilities { get; init; }
    public required IDictionary<string, string> Info { get; init; }
}