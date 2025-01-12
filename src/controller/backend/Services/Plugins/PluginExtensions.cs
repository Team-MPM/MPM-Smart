using PluginBase;

namespace Backend.Services.Plugins;

public static class PluginExtensions
{
    /// <summary>
    /// Should be called after all plugins are registered
    /// </summary>
    /// <param name="webApplication"></param>
    public static async Task StartPluginSystemAsync(this WebApplication webApplication)
    {
        var pluginManager = webApplication.Services.GetRequiredService<IPluginManager>();
        pluginManager.MapPlugins(webApplication);
        pluginManager.ConfigureServices();
        await pluginManager.StartAsync();
    }

    /// <summary>
    /// Load all plugins from the Plugin Directory
    /// </summary>
    /// <param name="webApplication"></param>
    /// <returns>Task representing the Plugin Loading process</returns>
    public static async Task LoadPluginsAsync(this WebApplication webApplication)
    {
        var pluginLoader = webApplication.Services.GetRequiredService<IPluginLoader>();
        await pluginLoader.LoadDefaultPluginsAsync();
    }
}