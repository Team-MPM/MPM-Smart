using System.Diagnostics;
using System.Security.Claims;
using ApiSchema.Identity;
using Backend.Services.Identity;
using Backend.Services.Plugins;
using Data.System;
using Microsoft.AspNetCore.Identity;
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
    private UserManager<SystemUser> m_UserManager = null!;
    private RoleManager<IdentityRole> m_RoleManager = null!;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        m_DbContext = scope.ServiceProvider.GetRequiredService<SystemDbContext>();
        m_UserManager = scope.ServiceProvider.GetRequiredService<UserManager<SystemUser>>();
        m_RoleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
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
       
        const string admin = "admin";
        
        var adminRole = await m_RoleManager.FindByNameAsync(admin);
        
        if (adminRole is null)
        {
            var result = await m_RoleManager.CreateAsync(new IdentityRole(admin));
            
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin role: {@Error}", result.Errors);
            }

            var role = await m_RoleManager.FindByNameAsync("admin");
            if (role is not null)
            {
                await m_RoleManager.AddClaimAsync(role, new Claim("Permissions", UserClaims.AllOnUser));
                await m_RoleManager.AddClaimAsync(role, new Claim("Permissions", UserClaims.ViewSettings));
            }
        }
        
        var adminUser = await m_UserManager.FindByNameAsync(admin);

        if (adminUser is null)
        {
            var result = await m_UserManager.CreateAsync(new SystemUser
            {
                Email = null,
                UserName = admin,
                UserProfile = new UserProfileEntity()
            }, admin);
            
            if (!result.Succeeded)
            {
                logger.LogError("Failed to create admin user: {@Error}", result.Errors);
            }
            
            adminUser = await m_UserManager.FindByNameAsync(admin);
            
            result = await m_UserManager.AddToRoleAsync(adminUser!, admin);
            // await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.AllPermissions)); //TODO change this later

            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.AllOnUser));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.AllPermissions));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.AllOnProfile));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.ViewSettings));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.ChangeHostName));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.ChangeProfilePicture));
            await m_UserManager.AddClaimAsync(adminUser!, new Claim("Permissions", UserClaims.EditProfile));

            if (!result.Succeeded)
            {
                logger.LogError("Failed to add admin user to admin role: {@Error}", result.Errors);
            }
        }

        // Guess user

        if (await m_UserManager.FindByNameAsync("visitor") is null)
        {
            var result = await m_UserManager.CreateAsync(new SystemUser()
            {
                UserName = "Visitor",
                UserProfile = new UserProfileEntity()
            });
        }

        // System Configuration

        if(!await m_DbContext.SystemConfiguration.AnyAsync(cancellationToken))
        {
            await m_DbContext.SystemConfiguration.AddAsync(new SystemConfiguration()
            {
                SystemName = "Controller",
                TimeZone = "UTC",
                TimeBetweenDataUpdatesSeconds = 5
            }, cancellationToken);
            logger.LogInformation("System Configuration seeded");
        }

        if (env.IsDevelopment())
        {
            
        }

        await m_DbContext.SaveChangesAsync(cancellationToken);
    }
}