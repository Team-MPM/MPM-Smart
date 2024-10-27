using System.Diagnostics;
using System.Runtime.Loader;
using PluginBase;

namespace Backend.Services;

public class PluginLoader(
    IServiceProvider serviceProvider,
    ILogger<PluginLoader> logger
) : BackgroundService, IDisposable
{
    public const string ActivitySourceName = nameof(PluginLoader);

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);
    private readonly AssemblyLoadContext m_LoadContext = new AssemblyLoadContext(nameof(PluginLoader), true);
    private readonly TaskCompletionSource m_PluginsLoaded = new();

    public Task WaitForPluginsToLoadAsync() => m_PluginsLoaded.Task;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var pluginManager = serviceProvider.GetRequiredService<PluginManager>();

        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "plugins");

        foreach (var pluginDirectory in Directory.EnumerateDirectories(pluginsDirectory))
        {
            foreach (var dllPath in Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                var assembly = m_LoadContext.LoadFromAssemblyPath(dllPath);
                logger.LogInformation("Loaded assembly {AssemblyName}", assembly.FullName);

                foreach (var plugin in assembly.DefinedTypes.Where(t =>
                             t.GetInterfaces().Any(i => i.FullName == typeof(IPlugin).FullName) == true))
                {
                    // var constructor = plugin.GetConstructor(Type.EmptyTypes);
                    // if (constructor is null)
                    // {
                    //     logger.LogError("Failed to find constructor for plugin {PluginName}", plugin.FullName);
                    //     continue;
                    // }
                    //
                    // var instance = constructor.Invoke(null);
                    
                    var instance = Activator.CreateInstance(plugin.AsType());
                    
                    // if (instance is not IPlugin pluginInstance)
                    // {
                    //     logger.LogError("Failed to create instance of plugin {PluginName}", plugin.FullName);
                    //     continue;
                    // }

                    var type = instance.GetType();
                    
                    pluginManager.RegisterPlugin((IPlugin)instance);
                }
            }
        }

        logger.LogInformation("Plugin System initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);

        m_PluginsLoaded.SetResult();
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        m_LoadContext.Unload();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.WaitForFullGCComplete();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}