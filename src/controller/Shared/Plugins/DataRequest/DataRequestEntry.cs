namespace Shared.Plugins.DataRequest;

public class DataRequestEntry
{
    public required string PluginName { get; set; }
    public required string DataPoint { get; set; }
    public required string RequestedDataType { get; set; }
    public required DateTime StartDate { get; set; } = DateTime.Now.AddDays(-10);
    public required DateTime EndDate { get; set; } = DateTime.Now;
}