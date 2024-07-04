using ModuleBase;

namespace CoreModule;

public class CoreModuleImpl : IModule
{
    public string Name { get; } = "CoreModule";
    public string Version { get; } = "1.0.0";
    public string Description { get; } = "Core module for Smart home the system";
    public string Author { get; } = "Team-Mpm";
    
    public bool IsRunning { get; private set; }
    public bool IsEnabled { get; private set; }
    
    public IModule[] Dependencies { get; } = [];
    
    public bool Init()
    {
        IsRunning = true;
        IsEnabled = true;
        Console.WriteLine("CoreModule initialized");
        return true;
    }

    public bool Start()
    {
        throw new NotImplementedException();
    }

    public bool Stop()
    {
        throw new NotImplementedException();
    }

    public bool Enable()
    {
        throw new NotImplementedException();
    }

    public bool Disable()
    {
        throw new NotImplementedException();
    }
}