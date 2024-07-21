using System.Diagnostics;
using DataModel.Primary;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DbManager;

internal class DbInitializer(IWebHostEnvironment env, IServiceProvider serviceProvider, ILogger<DbInitializer> logger)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private readonly ActivitySource m_ActivitySource = new(ActivitySourceName);

    public Dictionary<string, object> Info { get; } = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Info["Started"] = DateTime.UtcNow;
        
        using var scope = serviceProvider.CreateScope();
        
        var primaryDbContext = scope.ServiceProvider.GetRequiredService<PrimaryDbContext>();
        await InitializeSqlDatabaseAsync(primaryDbContext, cancellationToken);
        await SeedPrimaryDbAsync(primaryDbContext, cancellationToken);

        var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
        var db = mongoClient.GetDatabase("HomeData");
        
        Info["Finished"] = DateTime.UtcNow;
        
        Info["Databases"] = new string[]
        {
            $"{primaryDbContext.Database.ProviderName}:{nameof(PrimaryDbContext)}"
        };
    }

    private async Task InitializeSqlDatabaseAsync(DbContext context, CancellationToken cancellationToken)
    {
        using var activity = m_ActivitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(context.Database.MigrateAsync, cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms",
            sw.ElapsedMilliseconds);
    }

    private async Task SeedPrimaryDbAsync(PrimaryDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding database");

        if (env.IsDevelopment())
        {
            
        }

        //await dbContext.SaveChangesAsync(cancellationToken);
    }
}