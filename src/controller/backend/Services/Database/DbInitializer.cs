using System.Diagnostics;
using Backend.Services.Plugins;
using Data.System;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Database;

public class DbInitializer(
    IWebHostEnvironment env, 
    IServiceProvider serviceProvider, 
    ILogger<DbInitializer> logger,
    IPluginManager pluginManager
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

        logger.LogInformation("System Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);

        await pluginManager.WaitForPluginInitializationAsync();

        logger.LogInformation("Starting Plugin System Database initialization after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);

        using var pluginScope = pluginManager.PluginServices!.CreateScope();
        var dbContextTypes = pluginScope.ServiceProvider.GetRequiredService<IServiceCollection>()
            .Where(sd => sd.ServiceType.IsAssignableTo(typeof(DbContext)))
            .Select(sd => sd.ServiceType);;

        foreach (var contextType in dbContextTypes)
        {
            var dbContext = (DbContext)pluginScope.ServiceProvider
                .GetRequiredService(contextType);
            var dbStrategy = dbContext.Database.CreateExecutionStrategy();
            await dbStrategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);
            logger.LogInformation("Database {DatabaseName} initialization completed after {ElapsedMilliseconds}ms",
                dbContext.Database.GetDbConnection().Database, sw.ElapsedMilliseconds);
        }

        logger.LogInformation("Plugin Database System initialization completed after {ElapsedMilliseconds}ms",
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