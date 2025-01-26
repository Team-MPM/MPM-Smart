using Shared;

namespace PluginBase.Services.Data;

public class DataIndexEntry
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

    public required Func<DataQuery, DataQueryResult> QueryHandler { get; set; }
}

public class DataIndex
{
    public Dictionary<Guid, DataIndexEntry> Entries { get; set; } = new();

    public void Add(DataIndexEntry entry)
    {
        if (DataTypeHelper.IsCombo(entry.QueryType))
        {
            if (entry.ComboOptions is null && entry.ComboOptionsSource is null)
                throw new InvalidOperationException(
                    "Either ComboOptions or ComboOptionsSource have to be set when combo type is specified");

            if (entry.ComboOptions is not null && entry.ComboOptionsSource is not null)
                throw new InvalidOperationException(
                    "ComboOptions and ComboOptionsSource are mutually exclusive");
        }

        if (DataTypeHelper.IsSeries(entry.QueryType))
        {
            if (entry.AvailableGranularity is null)
                throw new InvalidOperationException("AvailableGranularity must be set for type series");
        }

        Entries.Add(entry.Id, entry);
    }
}