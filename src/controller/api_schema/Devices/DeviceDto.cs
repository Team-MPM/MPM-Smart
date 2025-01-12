using Shared;

namespace ApiSchema.Devices;

public record DeviceDto(DeviceInfoDto DeviceInfo, DeviceState State, string? Location);