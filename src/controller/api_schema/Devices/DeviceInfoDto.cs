namespace ApiSchema.Devices;

public record DeviceInfoDto(
    string Name,
    string Serial,
    string Description,
    string Type,
    IDictionary<string, string> Capabilities,
    IDictionary<string, string> Parameters,
    IDictionary<string, string> Details);