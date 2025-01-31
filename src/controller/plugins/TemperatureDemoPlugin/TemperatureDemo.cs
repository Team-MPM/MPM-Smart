using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Data;
using PluginBase.Services.Options;
using PluginBase.Services.Permissions;
using Shared;
using TemperatureDemoPlugin.Data;
using TemperatureDemoPlugin.Permissions;

namespace TemperatureDemoPlugin;

public class TemperatureDemo : PluginBase<TemperatureDemo>
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
        services.AddDbContextPool<TemperatureDemoContext>(options =>
        {
            options.UseSqlite("Data Source=TemperatureDemo.db");
            options.EnableDetailedErrors();
        });
        return Task.CompletedTask;
    }

    protected override Task SystemStart()
    {
        var permissionProvider = ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        permissionProvider.AddRange("TemperatureDemo", TemperatureClaims.ExportPermissions());

        var index = ApplicationServices.GetRequiredService<DataIndex>();
        index.Add(new DataPoint
        {
            Name = "Temperature Singe",
            Description = "Temperature",
            QueryType = DataQueryType.ComboDouble,
            Unit = "°C",
            Plugin = this,
            ComboOptions = ["Kitchen", "Living Room", "Bed Room"],
            Permission = TemperatureClaims.ViewSensorData,
            QueryHandler = async query => new ComboQueryResult(new Dictionary<string, object>
            {
                ["Kitchen"] = 20.0d,
                ["Living Room"] = 22.0d,
                ["Bed Room"] = 18.0d
            })
        });

        index.Add(new DataPoint
        {
            Name = "Temperature",
            Description = "Temperature",
            QueryType = DataQueryType.ComboSeriesDouble,
            Unit = "°C",
            Plugin = this,
            AvailableGranularity =
                [TimeSpan.FromDays(1), TimeSpan.FromHours(4), TimeSpan.FromHours(1), TimeSpan.FromMinutes(10)],
            ComboOptions = ["Kitchen", "Living Room", "Bed Room"],
            Permission = TemperatureClaims.ViewSensorData,
            QueryHandler = async query => new ComboSeriesQueryResult(new Dictionary<string, object[]>
            {
                ["Kitchen"] = [20.0d, 21.0d, 22.0d, 23.0d],
                ["Living Room"] = [22.0d, 23.0d, 24.0d, 25.0d],
                ["Bed Room"] = [18.0d, 19.0d, 20.0d, 21.0d]
            })
        });
        return Task.CompletedTask;
    }

    protected override Task OnOptionBuilding(OptionsBuilder builder)
    {
        return Task.CompletedTask;
    }
}