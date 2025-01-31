using Shared;

namespace PluginBase.Services.Data;

public class DataPoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required IPlugin Plugin { get; set; }
    public required DataQueryType QueryType { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Unit { get; set; }
    public TimeSpan[]? AvailableGranularity { get; set; }
    public string[]? ComboOptions { get; set; }
    public Func<string[]>? ComboOptionsSource { get; set; }
    public required string Permission { get; set; }

    public required Func<DataQuery, Task<DataQueryResult>> QueryHandler { get; set; }
}