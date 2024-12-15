using Microsoft.EntityFrameworkCore;

namespace TemperatureDemoPlugin.Data;

public class TemperatureDemoContext(DbContextOptions<TemperatureDemoContext> options) : DbContext(options)
{
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<DataEntry> DataEntries { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataEntry>()
            .HasOne<Sensor>(s => s.Sensor)
            .WithMany(s => s.DataEntries)
            .HasForeignKey(s => s.SensorId);


        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TemperatureDemoContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}