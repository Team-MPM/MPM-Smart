using System.Text.Json;

namespace PluginBase.Services.Devices;

public class DeviceRegistry
{
    public const string BasePath = "./devices";

    public IReadOnlyList<Device> Devices => m_Devices.AsReadOnly();

    private readonly List<Device> m_Devices = [];
    private readonly DeviceTypeRegistry m_DeviceTypeRegistry;

    public DeviceRegistry(DeviceTypeRegistry deviceTypeRegistry)
    {
        m_DeviceTypeRegistry = deviceTypeRegistry;

        if (!Directory.Exists(BasePath))
            Directory.CreateDirectory(BasePath);

        foreach (var file in Directory.GetFiles(BasePath))
        {
            var json = File.ReadAllText(file);
            var device = JsonSerializer.Deserialize<Device>(json);
            m_Devices.Add(device!);
        }
    }

    public void RegisterDevice(Device device)
    {
        if (m_Devices.Any(d => d.Info.Serial == device.Info.Serial))
            return;

        var path = Path.Combine(BasePath, $"{device.Info.Serial}.json");
        var json = JsonSerializer.Serialize(device);
        File.WriteAllText(path, json);
    }

    public void UnregisterDevice(Device device)
    {
        var path = Path.Combine(BasePath, $"{device.Info.Serial}.json");
        File.Delete(path);
    }
}