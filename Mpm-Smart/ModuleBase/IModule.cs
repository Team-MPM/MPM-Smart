namespace ModuleBase;

public interface IModule
{
    public bool Init(IServiceProvider serviceProvider);
    public bool Start();
    public bool Stop();
    public bool Enable();
    public bool Disable();
    
    public bool IsRunning { get; }
    public bool IsEnabled { get; }
    
    public string Name { get; }
    public string Version { get; }
    public string Description { get; }
    public string Author { get; }
    
    public IModule[] Dependencies { get; }
}