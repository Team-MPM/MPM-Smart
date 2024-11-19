namespace PluginBase;

public record PluginMetadata
{
    public string Name { get; init; } = string.Empty;
    public string RegistryName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;
}