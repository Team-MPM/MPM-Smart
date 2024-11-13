using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelemetryPlugin.Data;

public class CounterMetricEntry : MetricEntry
{
    public double Value { get; set; }
}

public class CounterMetricEntryConfiguration : IEntityTypeConfiguration<CounterMetricEntry>
{
    public void Configure(EntityTypeBuilder<CounterMetricEntry> builder)
    {
        builder.ToTable("CounterMetricEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Value).IsRequired();
        builder.HasOne(e => e.Metric)
            .WithMany(m => m.CounterEntries)
            .HasForeignKey(e => e.MetricId);
        builder.Property(e => e.MetricId).IsRequired();
        builder.Property(e => e.TimeStampStartUtc).IsRequired();
        builder.Property(e => e.TimeStampEndUtc).IsRequired();
    }
}