using Microsoft.EntityFrameworkCore;
using TelemetryPlugin.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<TelemetryDbContext>(options =>
{
    options.UseSqlite("Data Source=telemetry.db", sqliteOptions =>
        sqliteOptions.MigrationsAssembly(typeof(TelemetryDbContext).Assembly.FullName));
    options.EnableDetailedErrors();
});

await builder.Build().RunAsync();