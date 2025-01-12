namespace Shared.Plugins.DataResponse;

public class DataResponseInfo
{
    public required bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public required string PluginName { get; set; }
    public required string DataPoint { get; set; }
    public required string DataName { get; set; }
    public required string DataType { get; set; }
    public List<DataResponseEntry>? Data { get; set; }
}