using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModuleBase;

namespace CoreModule;

public class CoreModuleImpl : IModule
{
    public string Name { get; } = "CoreModule";
    public string Version { get; } = "1.0.0";
    public string Description { get; } = "Core module for Smart home the system";
    public string Author { get; } = "Team-Mpm";

    public bool IsRunning { get; private set; } = false;
    public bool IsEnabled { get; private set; } = true;
    
    public IModule[] Dependencies { get; } = [];

    private IServiceProvider m_ServiceProvider = null!;
    private ILogger<CoreModuleImpl> m_Logger = null!;
    
    public bool Init(IServiceProvider serviceProvider)
    {
        m_ServiceProvider = serviceProvider;
        m_Logger = serviceProvider.GetRequiredService<ILogger<CoreModuleImpl>>();
        m_Logger.LogInformation("CoreModule initialized");
        return true;
    }

    public bool Start()
    {
        m_Logger.LogInformation("CoreModule started");
        IsRunning = true;
        return true;
    }

    public bool Stop()
    {
        m_Logger.LogInformation("CoreModule stopped");
        IsRunning = false;
        return true;
    }

    public bool Enable()
    {
        IsEnabled = true;
        return true;
    }

    public bool Disable()
    {
        if (IsRunning) return false;
        IsEnabled = false;
        return true;
    }
}