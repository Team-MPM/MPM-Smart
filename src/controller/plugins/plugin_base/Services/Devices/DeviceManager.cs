using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PluginBase.Services.Devices;

public class DeviceManager(
    DeviceRegistry deviceRegistry,
    ILogger<DeviceManager> logger) : BackgroundService
{
    public IReadOnlyList<Device> ConnectedDevices => m_Devices.AsReadOnly();

    private readonly List<Device> m_Devices = [];

    public void AddConnectedDevice(Device device)
    {
        m_Devices.Add(device);
    }

    public bool ConnectToDevice(DeviceInfo deviceInfo)
    {
        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // await Task.Delay(2000, stoppingToken);
        //
        // // Example of how to construct a device object
        // var testDevice = new Device
        // {
        //     Info = new DeviceInfo
        //     {
        //         Name = "Test Device",
        //         Serial = "1234",
        //         Type = typeRegistry.GetRegisteredDevices().First(),
        //         Description = "hi",
        //         Parameters = new Dictionary<string, object>(),
        //         Capabilities = new Dictionary<string, string>()
        //         {
        //             { "test", "test" }
        //         },
        //         Info = new Dictionary<string, string>()
        //         {
        //             { "ip", "1.1.1.1" }
        //         }
        //     },
        //     State = DeviceState.Disabled,
        //     MetaData = new DeviceMeta
        //     {
        //         Location = "Test Location",
        //         ConnectionDetails = new Dictionary<string, string>()
        //         {
        //             { "key", "secret" }
        //         }
        //     }
        // };
        //
        // deviceRegistry.RegisterDevice(testDevice);

        while (true)
        {
            await Task.Delay(5000, stoppingToken);

            foreach (var device in deviceRegistry.Devices)
                try
                {
                    await device.Info.Type.PollAsync(device);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to poll device {Serial}", device.Info.Serial);
                }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}