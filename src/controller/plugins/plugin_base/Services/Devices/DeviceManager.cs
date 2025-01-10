using Microsoft.Extensions.Hosting;
using Shared;

namespace PluginBase.Services.Devices;

public class DeviceManager(DeviceTypeRegistry typeRegistry, DeviceRegistry deviceRegistry) : BackgroundService
{
    public IReadOnlyList<Device> ConnectedDevices => m_Devices.AsReadOnly();

    private readonly List<Device> m_Devices = [];

    public void AddConnectedDevice(Device device)
    {
        m_Devices.Add(device);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(2000, stoppingToken);

        var testDevice = new Device
        {
            Info = new DeviceInfo
            {
                Name = "Test Device",
                Serial = "1234",
                Type = typeRegistry.GetRegisteredDevices().First(),
                Description = "hi",
                Parameters = new Dictionary<string, object>(),
                Capabilities = new Dictionary<string, string>()
                {
                    { "test", "test" }
                },
                Info = new Dictionary<string, string>()
                {
                    { "ip", "1.1.1.1" }
                }
            },
            State = DeviceState.Disabled,
            MetaData = new DeviceMeta
            {
                Location = "Test Location",
                ConnectionDetails = new Dictionary<string, string>()
                {
                    { "key", "secret" }
                }
            }
        };

        deviceRegistry.RegisterDevice(testDevice);

        while (true)
        {
            await Task.Delay(5000, stoppingToken);


            // foreach (var deviceType in registry.GetRegisteredDevices())
            // {
            //     foreach (var device in m_Devices.Where(d => d.Info.Type == deviceType).ToList())
            //     {
            //         var suc = await deviceType.PollAsync(device);
            //         if (!suc)
            //         {
            //             m_Devices.Remove(device);
            //         }
            //     }
            //
            //     await deviceType.ReconnectAsync(sp);
            // }
        }
    }
}