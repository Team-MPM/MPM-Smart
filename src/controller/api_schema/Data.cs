using Shared;

namespace ApiSchema;

public record class DataIndexEntryDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PluginName { get; set; }
    public required DataQueryType QueryType { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Unit { get; set; }
    public TimeSpan[]? AvailableGranularity { get; set; }
    public string[]? ComboOptions { get; set; }
    public required string Permission { get; set; }
}


public record DataQueryDto(
    Guid Id, 
    TimeSpan? Granularity, 
    DateTime? From, 
    DateTime? To, 
    string[]? ComboOptions, 
    string? Filter
);

public record DataQueryResultDto(
    DataQueryResultType Type, 
    object Data
);

public record SingleDataQueryResultDto(
    DataQueryResultType Type, 
    object Data
);

public record SeriesDataQueryResultDto(
    DataQueryResultType Type, 
    object[] Data
);

public record ComboSeriesDataQueryResultDto(
    DataQueryResultType Type, 
    Dictionary<string, object[]> Data
);