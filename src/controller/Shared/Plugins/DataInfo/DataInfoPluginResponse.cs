namespace Shared.Plugins.DataInfo;

public class DataInfoPluginResponse
{
    public required bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public required List<DataInfoSensorEntry> SensorEntries { get; set; }
}