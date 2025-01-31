using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using MpmSmart.PluginTemplate.Endpoints;
using PluginBase.Services.Options;

namespace MpmSmart.PluginTemplate;

public class TestPluginClass : PluginBase<TestPluginClass>
{
    protected override Task Initialize()
    {
        return Task.CompletedTask;
    }

    protected override Task BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapTestEndpoints();
        return Task.CompletedTask;
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        //services.AddDbContextPool
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        return Task.CompletedTask;
    }
}