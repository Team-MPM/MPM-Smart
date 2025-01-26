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
    protected override void Initialize()
    {
    }

    protected override void BuildEndpoints(IEndpointRouteBuilder routeBuilder)
    {
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContextPool<TemperatureDemoContext>(options =>
        {
            options.UseSqlite("Data Source=TemperatureDemo.db");
            options.EnableDetailedErrors();
        });
    }

    protected override void SystemStart()
    {
        var permissionProvider = ApplicationServices.GetRequiredService<AvailablePermissionProvider>();
        permissionProvider.AddRange("TemperatureDemo", TemperatureClaims.ExportPermissions());

        var index = ApplicationServices.GetRequiredService<DataIndex>();
        index.Add(new DataIndexEntry()
        {
            Name = "Temperature Singe",
            Description = "Temperature",
            QueryType = DataQueryType.Double,
            Unit = "°C",
            Plugin = this,
            Permission = TemperatureClaims.ViewSensorData,
            QueryHandler = query => { return null; }
        });

        index.Add(new DataIndexEntry()
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
            QueryHandler = query => { return null; }
        });
    }

    protected override void OnOptionBuilding(OptionsBuilder builder)
    {
    }
}