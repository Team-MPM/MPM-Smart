using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.Loader;
using PluginBase;

namespace Backend.Services.Plugins;

public class PluginLoader(
    IServiceProvider serviceProvider,
    IWebHostEnvironment env,
    ILogger<PluginLoader> logger,
    IFileSystem fileSystem
) : BackgroundService, IPluginLoader
{
    public const string ActivitySourceName = nameof(PluginLoader);

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);
    private readonly AssemblyLoadContext m_LoadContext = new(nameof(PluginLoader), true);
    private readonly TaskCompletionSource m_PluginsLoaded = new();

    public List<Assembly> PluginAssemblies { get; } = [];

    public Task WaitForPluginsToLoadAsync() => m_PluginsLoaded.Task;
    
    public async Task LoadDefaultPluginsAsync()
    {
        await ExecuteAsync(CancellationToken.None);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var pluginManager = serviceProvider.GetRequiredService<PluginManager>();
        var pluginsDirectory = Path.Combine(env.ContentRootPath, "..", "..", "plugins");
        
        if (!fileSystem.Directory.Exists(pluginsDirectory))
        {
            logger.LogWarning("No plugins directory found at {PluginsDirectory}", pluginsDirectory);
            m_PluginsLoaded.SetResult();
            return Task.CompletedTask;
        }
        
        logger.LogInformation("Loading Plugin from Directory: {PluginDirectory}", pluginsDirectory);

        LoadPluginsFromDirectory(pluginsDirectory, pluginManager);

        logger.LogInformation("Plugin System initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);

        m_PluginsLoaded.SetResult();
        return Task.CompletedTask;
    }

    public void LoadPluginsFromDirectory(string pluginsDirectory, PluginManager pluginManager)
    {
        foreach (var pluginDirectory in fileSystem.Directory.EnumerateDirectories(pluginsDirectory))
        {
            foreach (var dllPath in fileSystem.Directory.EnumerateFiles(pluginDirectory, "*.dll"))
            {
                var assembly = m_LoadContext.LoadFromStream(fileSystem.File.OpenRead(dllPath));
                logger.LogInformation("Loaded assembly {AssemblyName}", assembly.FullName);
                PluginAssemblies.Add(assembly);
                
                foreach (var plugin in assembly.DefinedTypes.Where(t =>
                             t.GetInterfaces().Any(i => i.FullName == typeof(IPlugin).FullName) == true))
                {
                    var instance = Activator.CreateInstance(plugin.AsType());
                    var pluginInstance = (IPlugin)instance!;
                    pluginManager.RegisterPlugin(pluginInstance, pluginDirectory);
                }
            }
        }
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