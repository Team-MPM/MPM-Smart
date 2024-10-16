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

                if (assembly.DefinedTypes.Any(t => t.BaseType?.GetInterfaces().Any(i => i.FullName == typeof(IPlugin).FullName) == true))
                    pluginManager.RegisterPlugin(assembly);
            }
        }

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);

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