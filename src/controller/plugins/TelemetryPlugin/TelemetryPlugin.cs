using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using TelemetryPlugin.Data;

namespace TelemetryPlugin;

public class TelemetryPlugin : PluginBase<TelemetryPlugin>
{
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/api/telemetry/test", () =>
        {
            if (Services is null) return Results.Problem("Plugin System still initializing",
                null, StatusCodes.Status500InternalServerError);

            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();
            var entry = db.TestEntities.Add(new TestEntity {Text = "Test Entry"});
            db.SaveChanges();
            return Results.Json(db.TestEntities.AsAsyncEnumerable());
        });
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TelemetryDbContext>(options =>
        {
            options.UseSqlite("Data Source=telemetry.db");
            options.EnableDetailedErrors();
        });
    }

    protected override void SystemStart()
    {
    }
}