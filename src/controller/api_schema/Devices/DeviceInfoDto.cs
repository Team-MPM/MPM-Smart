namespace ApiSchema.Devices;

public record DeviceInfoDto(
    string Name,
    string Description,
    IDictionary<string, object> Parameters,
    IDictionary<string, string> Capabilities);