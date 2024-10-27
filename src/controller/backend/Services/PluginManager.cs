using System.Reflection;
using PluginBase;

namespace Backend.Services;

public class PluginManager(ILogger<PluginManager> logger) : IDisposable
{
    public List<IPlugin> Plugins { get; } = [];

    public void RegisterPlugin(IPlugin plugin)
    {
        logger.LogInformation("Registered plugin {PluginName}", plugin.Name);
        plugin.Initialize();
        Plugins.Add(plugin);
    }

    public void Dispose()
    {
        foreach (var plugin in Plugins)
        {
            plugin.Dispose();
        }
    }
}