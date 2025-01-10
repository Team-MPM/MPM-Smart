namespace ApiSchema.Devices;

public record DeviceInfoDto(
    string Name,
    string Description,
    IDictionary<string, string> Capabilities);