using Microsoft.AspNetCore.Http;

namespace PluginBase.Services.Data;

public class DataQuery
{
    public required Guid Id { get; set; }
    public required TimeSpan? Granularity { get; set; }
    public required DateTime? From { get; set; }
    public required DateTime? To { get; set; }
    public required string[]? ComboOptions { get; set; }
    public required string? Filter { get; set; }

    public required IServiceProvider Services { get; set; }
    public required HttpContext Context { get; set; }
}