using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using MpmSmart.PluginTemplate.Endpoints;

namespace MpmSmart.PluginTemplate;

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