using System.Text.Json.Serialization;
using Shared;

namespace PluginBase.Services.Devices;

public class Device
{
    public required DeviceInfo Info { get; init; }

    [JsonIgnore]
    public DeviceState State { get; set; }

    public required DeviceMeta MetaData { get; init; }
}