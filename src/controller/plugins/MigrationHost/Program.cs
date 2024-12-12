using Microsoft.EntityFrameworkCore;
using TelemetryPlugin.Data;
using TemperatureDemoPlugin.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TelemetryDbContext>(options =>
{
    options.UseSqlite("Data Source=telemetry.db", sqliteOptions =>
        sqliteOptions.MigrationsAssembly(typeof(TelemetryDbContext).Assembly.FullName));
    options.EnableDetailedErrors();
});

builder.Services.AddDbContextPool<TemperatureDemoContext>(options =>
{
    options.UseSqlite("Data Source=TemperatureDemo.db", sqliteOptions =>
        sqliteOptions.MigrationsAssembly(typeof(TemperatureDemoContext).Assembly.FullName));
    options.EnableDetailedErrors();
});

await builder.Build().RunAsync();