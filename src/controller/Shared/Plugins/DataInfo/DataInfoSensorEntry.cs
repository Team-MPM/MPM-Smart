namespace Shared.Plugins.DataInfo;

public class DataInfoSensorEntry
{
    public required string SensorName { get; set; }
    public required List<string> RequestableDataTypes { get; set; }
}