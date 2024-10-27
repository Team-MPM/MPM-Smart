using Microsoft.AspNetCore.Routing;
using PluginBase;
using TestPlugin.Endpoints;

namespace TestPlugin;

public class TestPluginClass : IPlugin
{
    public Guid Guid => Guid.Parse("042ea4a0-f4ca-4d23-a23e-122bc89cadc5");
    public string Name => nameof(TestPlugin);
    public string Description => "A test plugin";
    public string Author => "Gabriel Martin";
    public string Version => "1.0.0";
    public string IconUrl => "https://mpm-smart.g-martin.work/icons/test_plugin.png";
    public string PluginUrl => "https://mpm-smart.g-martin.work/plugins/test_plugin";
    
    public IEnumerable<Action<IEndpointRouteBuilder>> Endpoints => [
        TestEndpoints.MapTestEndpoints
    ];

    public TestPluginClass()
    {
        
    }
    
    public void Initialize()
    {
        
    }
    
    public void Dispose()
    {
    }
}