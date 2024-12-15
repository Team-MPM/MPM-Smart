using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Services;

public class DbInitializer(
    IWebHostEnvironment env, 
    IServiceProvider serviceProvider, 
    ILogger<DbInitializer> logger
    ) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);
    private ServerDbContext m_DbContext = null!;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        m_DbContext = scope.ServiceProvider.GetRequiredService<ServerDbContext>();
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
       
        var defaultTags = new List<string>
        {
            "Controller",
            "Action",
            "Sensor",
            "Device",
            "Telemetry",
            "Core"
        };

        foreach (var tag in defaultTags)
        {
            if (!await m_DbContext.PluginTags.AnyAsync(t => t.Tag == tag, cancellationToken))
            {
                m_DbContext.PluginTags.Add(new PluginTag { Tag = tag });
            }
        }

        if (env.IsDevelopment())
        {
            
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }
}