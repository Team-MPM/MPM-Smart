namespace ApiSchema.Plugins;

public record PluginInfoDto(
    string Guid,
    string Name,
    string RegistryName,
    string Description,
    string Version,
    string Author,
    string Url,
    string IconUrl);