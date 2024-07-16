using DataModel.PrimaryDb;
using DbManager;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MPM_Betting.DbManager;

var assemblyName = typeof(Program).Assembly.GetName().Name;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<PrimaryDbContext>("PrimaryDatabase", null,
    optionsBuilder =>
    {
        optionsBuilder.UseSqlServer(sqlBuilder =>
        {
            sqlBuilder.MigrationsAssembly(assemblyName);
            sqlBuilder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableDetailedErrors();
    });

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(DbInitializer.ActivitySourceName));

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer");

var app = builder.Build();

app.UseRouting();
app.MapDefaultEndpoints();

// app.MapGet("/health/db-initializer", async (HealthCheckService  healthCheckService) =>
// {
//     var healthResult = await healthCheckService.;
// });

await app.RunAsync();
