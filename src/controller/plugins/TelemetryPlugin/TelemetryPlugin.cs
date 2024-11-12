using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using TelemetryPlugin.Data;
using TelemetryPlugin.Services;

namespace TelemetryPlugin;

public class TelemetryPlugin : PluginBase<TelemetryPlugin>
{
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TelemetryDbContext>(options =>
        {
            options.UseSqlite("Data Source=telemetry.db");
            options.EnableDetailedErrors();
        });

        services.AddSingleton<TelemetryDataProvider>();
        services.AddSingleton<TelemetryDataProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<TelemetryDataProcessor>());
    }

    protected override void SystemStart()
    {
    }
}