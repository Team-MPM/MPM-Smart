using System.Text.Json;

namespace PluginBase.Services.Devices;

public class DeviceRegistry
{
    private const string BasePath = "./devices";

    public IReadOnlyList<Device> Devices => m_Devices.AsReadOnly();

    private readonly List<Device> m_Devices = [];
    private readonly Task m_LoadTask;

    public DeviceRegistry(IPluginManager pluginManager)
    {
        m_LoadTask = Task.Run(async () =>
        {
            await pluginManager.PluginInitializationComplete();
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            foreach (var file in Directory.GetFiles(BasePath))
            {
                await using var fs = File.OpenRead(file);
                var device = await JsonSerializer.DeserializeAsync<Device>(fs);
                m_Devices.Add(device!);
            }
        });
    }

    public async Task RegisterDeviceAsync(Device device)
    {
        await m_LoadTask;
        if (m_Devices.Any(d => d.Info.Serial == device.Info.Serial))
            return;

        var path = Path.Combine(BasePath, $"{device.Info.Serial}.json");
        await using var fs = new FileStream(path, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(fs, device);
    }

    public async Task PersistAllAsync()
    {
        await m_LoadTask;
        foreach (var device in m_Devices)
        {
            var path = Path.Combine(BasePath, $"{device.Info.Serial}.json");
            await using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await JsonSerializer.SerializeAsync(fs, device);
        }
    }

    public async Task UnregisterDeviceAsync(Device device)
    {
        await m_LoadTask;
        m_Devices.Remove(device);
        var path = Path.Combine(BasePath, $"{device.Info.Serial}.json");
        File.Delete(path);
    }
}