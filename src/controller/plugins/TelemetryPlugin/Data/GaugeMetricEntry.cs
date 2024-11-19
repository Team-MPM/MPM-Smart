using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelemetryPlugin.Data;

public class GaugeMetricEntry : MetricEntry
{
    public double Value { get; set; }
}

public class GaugeMetricEntryConfiguration : IEntityTypeConfiguration<GaugeMetricEntry>
{
    public void Configure(EntityTypeBuilder<GaugeMetricEntry> builder)
    {
        builder.ToTable("GaugeMetricEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Value).IsRequired();
        builder.HasOne(e => e.Metric)
            .WithMany(m => m.GaugeEntries)
            .HasForeignKey(e => e.MetricId);
        builder.Property(e => e.MetricId).IsRequired();
        builder.Property(e => e.TimeStampStartUtc).IsRequired();
        builder.Property(e => e.TimeStampEndUtc).IsRequired();
    }
}