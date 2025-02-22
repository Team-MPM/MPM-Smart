namespace PluginBase.Services.Devices;

public record DeviceInfo
{
    public required string Name { get; init; }
    public required string Serial { get; init; }
    public required string Description { get; init; }
    public required IDeviceType Type { get; init; }
    public required IDictionary<string, string> Capabilities { get; init; }
    public required IDictionary<string, string> Details { get; init; }
}