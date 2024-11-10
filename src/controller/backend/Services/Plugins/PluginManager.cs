using PluginBase;

namespace Backend.Services.Plugins;

public class PluginManager(
    IServiceProvider sp,
    ILogger<PluginManager> logger,
    IWebHostEnvironment env
) : IDisposable
{
    public List<IPlugin> Plugins { get; } = [];
    public ServiceProvider? PluginServices { get; set; }


    /// <summary>
    /// Register a plugin with the PluginManager
    /// </summary>
    /// <param name="plugin">Plugin instance to register</param>
    /// <param name="path">Plugin assembly Path</param>
    /// <returns>Registration successful</returns>
    public bool RegisterPlugin(IPlugin plugin, string path)
    {
        logger.LogInformation("Registered plugin {Id}", plugin.Guid.ToString());

        try
        {
            plugin.OnInitialize(sp, path, env.ContentRootPath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to initialize plugin {Id} from {Path}",
                plugin.Guid.ToString(), path);
            return false;
        }

        Plugins.Add(plugin);
        return true;
    }

    public void MapPlugins(IEndpointRouteBuilder routeBuilder)
    {
        foreach (var plugin in Plugins)
        {
            plugin.OnEndpointBuilding(routeBuilder);
        }
    }

    public void Dispose()
    {
        if (PluginServices is not null)
        {
            foreach (var hostedService in PluginServices.GetServices<IHostedService>())
            {
                hostedService.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
        }


        foreach (var plugin in Plugins)
        {
            plugin.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    public void ConfigureServices()
    {
        var services = new ServiceCollection();

        foreach (var plugin in Plugins)
        {
            plugin.OnConfiguring(services);
        }

        PluginServices = services.BuildServiceProvider();
    }

    /// <summary>
    /// Should be called after all plugins are registered and configured.
    /// Starts the Plugin System and launches all registered Services.
    /// </summary>
    /// <exception cref="InvalidOperationException">Plugin services aren't configured</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (PluginServices is null)
            throw new InvalidOperationException("Plugin Services not configured");

        foreach (var plugin in Plugins)
            plugin.OnSystemStart(PluginServices);

        var hostedServices = PluginServices.GetServices<IHostedService>();

        foreach (var hostedService in hostedServices)
            await hostedService.StartAsync(cancellationToken);

        logger.LogInformation("Plugin System started");
    }
}