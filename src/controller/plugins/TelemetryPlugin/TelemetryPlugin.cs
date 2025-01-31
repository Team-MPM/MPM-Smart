using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Options;
using TelemetryPlugin.Data;
using TelemetryPlugin.Services;

namespace TelemetryPlugin;

public class TelemetryPlugin : PluginBase<TelemetryPlugin>
{
    protected override Task Initialize()
    {
        return Task.CompletedTask;
    }

    protected override Task BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        return Task.CompletedTask;
    }

    protected override Task ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TelemetryDbContext>(options =>
        {
            options.UseSqlite("Data Source=telemetry.db");
            options.EnableDetailedErrors();
        });

        services.AddSingleton<TelemetryDataProvider>();
        services.AddSingleton<TelemetryDataProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<TelemetryDataProcessor>());
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