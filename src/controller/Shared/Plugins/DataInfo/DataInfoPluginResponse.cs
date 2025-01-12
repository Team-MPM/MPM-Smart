namespace Shared.Plugins.DataInfo;

public class DataInfoPluginResponse
{
    public required bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public required List<DataPointEntry> SensorEntries { get; set; }
}