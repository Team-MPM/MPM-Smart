using Microsoft.EntityFrameworkCore;

namespace TelemetryPlugin.Data;

public class TelemetryDbContext(DbContextOptions<TelemetryDbContext> options) : DbContext(options)
{
    public DbSet<Metric> Metrics { get; set; }
    public DbSet<GaugeMetricEntry> GaugeMetricEntries { get; set; }
    public DbSet<HistogramBucketMetricEntry> HistogramBucketMetricEntries { get; set; }
    public DbSet<HistogramSumMetricEntry> HistogramSumMetricEntries { get; set; }
    public DbSet<CounterMetricEntry> CounterMetricEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TelemetryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}