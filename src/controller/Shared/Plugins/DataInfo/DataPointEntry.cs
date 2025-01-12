namespace Shared.Plugins.DataInfo;

public class DataPointEntry
{
    public required string DataPoint { get; set; }
    public required List<string> RequestableDataTypes { get; set; }
}