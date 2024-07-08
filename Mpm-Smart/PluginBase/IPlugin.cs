namespace PluginBase;

public interface IPlugin
{
    public bool Init(IServiceProvider serviceProvider);
    
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Author { get; }
}
