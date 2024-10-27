using Microsoft.AspNetCore.Routing;

namespace PluginBase;

public interface IPlugin : IDisposable
{
    Guid Guid { get; }
    string Name { get; }
    string Description { get; }
    string Author { get; }
    string Version { get; }
    string IconUrl { get; }
    string PluginUrl { get; }
    IEnumerable<Action<IEndpointRouteBuilder>> Endpoints { get; }
    
    void Initialize();
}