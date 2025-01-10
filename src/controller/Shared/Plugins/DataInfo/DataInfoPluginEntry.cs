namespace Shared.Plugins.DataInfo;

public class DataInfoPluginEntry
{
    public required bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public required string Plugin { get; set; }
    public required string SensorName { get; set; }
    public required List<string> RequestableDataTypes { get; set; }
}