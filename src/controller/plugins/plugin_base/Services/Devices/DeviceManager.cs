using Microsoft.Extensions.Hosting;

namespace PluginBase.Services.Devices;

public class DeviceManager(DeviceTypeRegistry registry, IServiceProvider sp) : BackgroundService
{
    public IReadOnlyList<Device> ConnectedDevices => m_Devices.AsReadOnly();

    private readonly List<Device> m_Devices = [];

    public void AddConnectedDevice(Device device)
    {
        m_Devices.Add(device);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            await Task.Delay(5000, stoppingToken);

            foreach (var deviceType in registry.GetRegisteredDevices())
            {
                foreach (var device in m_Devices.Where(d => d.Type == deviceType).ToList())
                {
                    var suc = await deviceType.PollAsync(device);
                    if (!suc)
                    {
                        m_Devices.Remove(device);
                    }
                }

                await deviceType.ReconnectAsync(sp);
            }
        }
    }
}