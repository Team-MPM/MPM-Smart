using DataModel.PrimaryDb;
using DbManager;
using Microsoft.EntityFrameworkCore;

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
    .AddCheck<DbInitializerHealthCheck>("DbInitializer")
    .AddSqlServer(builder.Configuration.GetConnectionString("PrimaryDatabase")!);

var app = builder.Build();

app.UseRouting();
app.MapDefaultEndpoints();

await app.RunAsync();
