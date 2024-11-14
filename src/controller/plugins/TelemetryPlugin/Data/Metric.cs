using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelemetryPlugin.Data;

public class Metric
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string MetricName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public MetricType Type { get; set; }
    public ICollection<GaugeMetricEntry>? GaugeEntries { get; set; }
    public ICollection<CounterMetricEntry>? CounterEntries { get; set; }
    public ICollection<HistogramBucketMetricEntry>? HistogramBucketEntries { get; set; }
    public ICollection<HistogramSumMetricEntry>? HistogramSumEntries { get; set; }
}

public class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metrics");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(200);
        builder.Property(m => m.MetricName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(5000);
        builder.Property(m => m.Unit).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Type).IsRequired().HasConversion<string>();
    }
}