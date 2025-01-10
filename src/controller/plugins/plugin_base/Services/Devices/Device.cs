using System.Text.Json.Serialization;
using Shared;

namespace PluginBase.Services.Devices;

public class Device
{
    public DeviceInfo Info { get; set; }

    [JsonIgnore]
    public DeviceState State { get; set; }

    public DeviceMeta MetaData { get; set; }
}