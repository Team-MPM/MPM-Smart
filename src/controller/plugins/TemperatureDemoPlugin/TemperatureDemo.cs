using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PluginBase;
using PluginBase.Services.Options;
using PluginBase.Services.Permissions;
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
    }

    protected override void OnOptionBuilding(OptionsBuilder builder)
    {

    }
}
