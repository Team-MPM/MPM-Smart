using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using TestPlugin.Endpoints;

namespace TestPlugin;

public class TestPluginClass : PluginBase<TestPluginClass>
{
    protected override void Initialize()
    {
        
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapTestEndpoints();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        //services.AddDbContextPool
    }

    protected override void SystemStart()
    {
        
    }
}