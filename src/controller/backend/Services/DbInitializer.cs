using System.Diagnostics;
using Data.System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class DbInitializer(
    IWebHostEnvironment env, 
    IServiceProvider serviceProvider, 
    ILogger<DbInitializer> logger
    ) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);
    private SystemDbContext m_DbContext = null!;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        m_DbContext = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
        await InitializeDatabaseAsync(cancellationToken);
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = m_DbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(m_DbContext.Database.MigrateAsync, cancellationToken);

        await SeedAsync(cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding database");
       
        if (env.IsDevelopment())
        {
            
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }
}