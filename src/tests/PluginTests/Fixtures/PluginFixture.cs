using System.Reflection;
using Backend.Services;
using Microsoft.Extensions.DependencyInjection;
using TestBase.Helpers;

namespace PluginTests.Fixtures;

public class PluginFixture
{
    public PluginLoader PluginLoader { get; private set; }
    public PluginManager PluginManager { get; private set; }
    public ServiceProvider ServiceProvider { get; private set; }

    public int PluginCount { get; private set; }
    
    public PluginFixture()
    {
        var pluginSourceDir = Path.Combine(Assembly.GetExecutingAssembly().Location,
            "../../../../../../../build/plugins");
        var pluginTargetDir = Path.Combine(Assembly.GetExecutingAssembly().Location, "../Plugins");
        var appPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "../temp/app");
        
        DirectoryHelpers.EnsureEmpty(pluginSourceDir);
        DirectoryHelpers.EnsureEmpty(pluginTargetDir);
        DirectoryHelpers.EnsureEmpty(appPath);
        
        DirectoryHelpers.Copy(pluginSourceDir, pluginTargetDir, true);
        Directory.SetCurrentDirectory(appPath);
        
        PluginCount = Directory.GetDirectories(pluginTargetDir).Length;
    }

    public void LoadPlugins()
    {
        var services = new ServiceCollection();

        services.AddSingleton<PluginManager>();
        services.AddSingleton<PluginLoader>();
        services.AddLogging();

        ServiceProvider = services.BuildServiceProvider();

        PluginLoader = ServiceProvider.GetRequiredService<PluginLoader>();
        PluginManager = ServiceProvider.GetRequiredService<PluginManager>();
        
        PluginLoader.StartAsync(CancellationToken.None);
        PluginLoader.WaitForPluginsToLoadAsync().Wait();
    }
}