using Shared;

namespace PluginBase.Services.Data;

public class DataPoint
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required IPlugin Plugin { get; init; }
    public required DataQueryType QueryType { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Unit { get; init; }
    public TimeSpan[]? AvailableGranularity { get; init; }
    public string[]? ComboOptions { get; init; }
    public Func<string[]>? ComboOptionsSource { get; init; }
    public required string Permission { get; init; }

    public required Func<DataQuery, Task<DataQueryResult?>> QueryHandler { get; init; }
    
    public async Task<DataQueryResult> ExecuteQuery(DataQuery query)
    {
        try
        {
            var result = await QueryHandler(query);
            return result ?? new FailedQueryResult("Query returned no data");
        }
        catch (Exception)
        {
            return new FailedQueryResult("Failed to execute query");
        }
      
    }
}