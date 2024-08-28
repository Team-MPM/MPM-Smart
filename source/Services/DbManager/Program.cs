using DataModel.Primary;
using DbManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var assemblyName = typeof(Program).Assembly.GetName().Name;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine(builder.Configuration.GetConnectionString("PrimaryDb"));

builder.Services.AddDbContextPool<PrimaryDbContext>(optionsBuilder =>
    {
        optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("PrimaryDb"), sqlBuilder =>
        {
            sqlBuilder.MigrationsAssembly(assemblyName);
            sqlBuilder.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });

        optionsBuilder.EnableDetailedErrors();
        
        if (builder.Environment.IsDevelopment())
            optionsBuilder.EnableSensitiveDataLogging();
    });

builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<DbInitializer>());

builder.Services.AddHealthChecks()
    .AddCheck<DbInitializerHealthCheck>("DbInitializer")
    .AddSqlServer(builder.Configuration.GetConnectionString("PrimaryDb")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

app.MapHealthChecks("/health");

await app.RunAsync();
