using System.Reflection;

namespace Backend.Services.Plugins;

public interface IPluginLoader : IDisposable
{
    public List<Assembly> PluginAssemblies { get; }
    public Task WaitForPluginsToLoadAsync();
    public Task LoadDefaultPluginsAsync();
}