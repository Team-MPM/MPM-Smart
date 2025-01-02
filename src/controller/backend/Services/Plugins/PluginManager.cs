using PluginBase;
using PluginBase.Services.Telemetry;
using Serilog;

namespace Backend.Services.Plugins;

public class PluginManager(
    IServiceProvider sp,
    ILogger<PluginManager> logger,
    IWebHostEnvironment env
) : IPluginManager
{
    public List<IPlugin> Plugins { get; } = [];
    public IServiceProvider? PluginServices { get; set; }

    private readonly TaskCompletionSource m_PluginsLoaded = new();

    public Task WaitForPluginInitializationAsync() => m_PluginsLoaded.Task;
    
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
        services.AddSingleton<IServiceCollection>(services);

        services.AddSingleton(sp.GetRequiredService<ITelemetryDataCollector>());

        services.AddLogging(options =>
        {
            options.ClearProviders();

            options.AddSerilog(new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger());

            options.AddSerilog(new LoggerConfiguration()
                .WriteTo.File("logs/plugin.log")
                .MinimumLevel.Information()
                .CreateLogger());

            options.AddSerilog(new LoggerConfiguration()
                .WriteTo.File("logs/plugin-error.log")
                .MinimumLevel.Error()
                .CreateLogger());
        });

        foreach (var plugin in Plugins)
        {
            plugin.OnConfiguring(services);
        }

        PluginServices = services.BuildServiceProvider();
    }

    public async Task StartAsync()
    {
        if (PluginServices is null)
            throw new InvalidOperationException("Plugin Services not configured");

        foreach (var plugin in Plugins)
            plugin.OnSystemStart(PluginServices);

        var hostedServices = PluginServices.GetServices<IHostedService>();

        foreach (var hostedService in hostedServices)
            await hostedService.StartAsync(CancellationToken.None);

        m_PluginsLoaded.SetResult();

        logger.LogInformation("Plugin System started");
    }
}