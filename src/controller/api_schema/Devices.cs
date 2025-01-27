using Shared;

namespace ApiSchema;

public record DeviceDto(DeviceInfoDto DeviceInfo, DeviceState State, string? Location);

public record DeviceInfoDto(
    string Name,
    string Serial,
    string Description,
    string Type,
    IDictionary<string, string> Capabilities,
    IDictionary<string, string> Parameters,
    IDictionary<string, string> Details);
    
public record SensorDto(string Name, string Serial, string Plugin);