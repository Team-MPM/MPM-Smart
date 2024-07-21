using Microsoft.EntityFrameworkCore;

namespace DataModel.Primary;

public class PrimaryDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLog { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PrimaryDbContext).Assembly);
    }
}