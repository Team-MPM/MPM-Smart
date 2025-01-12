using System.Reflection;

namespace PluginBase;

public interface IPluginLoader : IDisposable
{
    public List<Assembly> PluginAssemblies { get; }
    public Task WaitForPluginsToLoadAsync();
    public Task LoadDefaultPluginsAsync();
}