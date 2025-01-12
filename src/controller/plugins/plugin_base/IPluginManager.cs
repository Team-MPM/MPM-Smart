using Microsoft.AspNetCore.Routing;

namespace PluginBase;

public interface IPluginManager : IDisposable
{
    public List<IPlugin> Plugins { get; }

    public IServiceProvider? PluginServices { get; }

    /// <summary>
    /// Register a plugin with the PluginManager
    /// </summary>
    /// <param name="plugin">Plugin instance to register</param>
    /// <param name="path">Plugin assembly Path</param>
    /// <returns>Registration successful</returns>
    public bool RegisterPlugin(IPlugin plugin, string path);

    /// <summary>
    /// Map all registered plugins to the provided routeBuilder.
    /// Should be called after all plugins are registered.
    /// </summary>
    /// <param name="routeBuilder"></param>
    public void MapPlugins(IEndpointRouteBuilder routeBuilder);

    /// <summary>
    /// Should be called after all plugins are registered.
    /// </summary>
    public void ConfigureServices();

    /// <summary>
    /// Should be called after all plugins are registered and configured.
    /// Starts the Plugin System and launches all registered Services.
    /// </summary>
    /// <exception cref="InvalidOperationException">Plugin services aren't configured</exception>
    public Task StartAsync();

    /// <summary>
    /// Can be used to wait for the Plugin System to finish initializing.
    /// </summary>
    /// <returns>Task representing the wait operation</returns>
    public Task PluginInitializationComplete();
}