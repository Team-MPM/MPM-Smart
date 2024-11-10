using PluginBase;

namespace Backend.Services.Plugins;

public interface IPluginManager : IDisposable
{
    public List<IPlugin> Plugins { get; }

    public ServiceProvider? PluginServices { get; }

    /// <summary>
    /// Register a plugin with the PluginManager
    /// </summary>
    /// <param name="plugin">Plugin instance to register</param>
    /// <param name="path">Plugin assembly Path</param>
    /// <returns>Registration successful</returns>
    public bool RegisterPlugin(IPlugin plugin, string path);

    public void MapPlugins(IEndpointRouteBuilder routeBuilder);

    public void ConfigureServices();

    /// <summary>
    /// Should be called after all plugins are registered and configured.
    /// Starts the Plugin System and launches all registered Services.
    /// </summary>
    /// <exception cref="InvalidOperationException">Plugin services aren't configured</exception>
    public Task StartAsync();
}