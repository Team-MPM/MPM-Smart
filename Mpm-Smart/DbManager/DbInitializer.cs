using System.Collections.Concurrent;
using System.Diagnostics;
using DataModel.PrimaryDb;
using Microsoft.EntityFrameworkCore;

namespace DbManager;

internal class DbInitializer(IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);

    private PrimaryDbContext m_PrimaryDbContext;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        m_PrimaryDbContext = scope.ServiceProvider.GetRequiredService<PrimaryDbContext>();
        await InitializeDatabaseAsync(cancellationToken);
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = m_PrimaryDbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(m_PrimaryDbContext.Database.MigrateAsync, cancellationToken);

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

        //await dbContext.SaveChangesAsync(cancellationToken);
    }
}