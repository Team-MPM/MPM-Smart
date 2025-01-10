namespace PluginBase.Services.Devices;

public class DeviceMeta
{
    public string? Location { get; init; }
    public Dictionary<string, string> ConnectionDetails { get; set; } = [];
}