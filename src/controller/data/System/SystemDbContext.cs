using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.System;

public class SystemDbContext(DbContextOptions<SystemDbContext> options) : IdentityDbContext<SystemUser>(options)
{
    public DbSet<UserProfileEntity> UserProfiles { get; set; }
    public DbSet<SystemConfiguration> SystemConfiguration { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SystemUserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingsEntityConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}