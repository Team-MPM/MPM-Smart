namespace PluginBase.DeviceData;

public class DeviceInfo
{
    public required string Name { get; set; }
    public required List<Guid> RequiredPluginIds { get; set; }
}