using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PluginBase.Services.Devices;

public class DeviceTypeRegistry(IServiceProvider sp, ILogger<DeviceTypeRegistry> logger)
{
    private readonly HashSet<IDeviceType> m_RegisteredDevices = [];

    /// <summary>
    /// Registers a device type if it hasn't already been registered.
    /// </summary>
    /// <typeparam name="TDevice">The device types to register.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the device type is already registered.</exception>
    public void RegisterDeviceType<TDevice>(TDevice device) where TDevice : IDeviceType
    {
        if (!m_RegisteredDevices.Add(device))
            throw new InvalidOperationException($"Device type '{typeof(TDevice).Name}' is already registered.");
    }

    /// <summary>
    /// Checks if a device type is registered.
    /// </summary>
    /// <typeparam name="TDevice">The device type to check.</typeparam>
    /// <returns>True if the device type is registered, otherwise false.</returns>
    public bool IsDeviceTypeRegistered<TDevice>() where TDevice : IDeviceType =>
        m_RegisteredDevices.Any(d => d.GetType() == typeof(TDevice));

    /// <summary>
    /// Gets all registered device types.
    /// </summary>
    /// <returns>A list of registered device types.</returns>
    public IEnumerable<IDeviceType> GetRegisteredDevices() => m_RegisteredDevices.AsEnumerable();

    /// <summary>
    /// Get a specific device type.
    /// </summary>
    /// <returns>Device Type</returns>
    public IDeviceType? GetDeviceType<TDevice>() where TDevice : IDeviceType =>
        m_RegisteredDevices.FirstOrDefault(d => d.GetType() == typeof(TDevice));

    /// <summary>
    /// Iterate over all registered device types and yield the devices found.
    /// </summary>
    /// <returns>Async Enumerable - use 'await foreach'!</returns>
    public async IAsyncEnumerable<DeviceInfo> ScanDevices()
    {
        foreach (var device in m_RegisteredDevices)
        {
            logger.LogInformation("Scanning for devices of type {DeviceType}", device.GetType().Name);
            await foreach (var deviceInfo in device.ScanAsync(sp))
            {
                logger.LogInformation("Found device {DeviceName}", deviceInfo.Name);
                yield return deviceInfo;
            }
        }
    }
}