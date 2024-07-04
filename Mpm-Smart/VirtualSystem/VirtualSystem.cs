using Microsoft.Extensions.Logging;
using VirtualSystem.Networking;

namespace VirtualSystem;

public class VirtualSystem : SystemBase.SystemBase
{
    private readonly ILogger<VirtualSystem> m_Logger;
    
    public VirtualSystem(ILogger<VirtualSystem> logger) : base(new Connectivity())
    {
        m_Logger = logger;
        
        m_Logger.LogInformation("VirtualSystem initializing...");
        
        if (!Connectivity.Init())
        {
            logger.LogCritical("Failed to initialize connectivity");
            State = SystemBase.SystemState.Stopped;
            return;
        }
        
        State = SystemBase.SystemState.Running;
    }
}