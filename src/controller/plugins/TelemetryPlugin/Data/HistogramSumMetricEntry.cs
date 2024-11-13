using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelemetryPlugin.Data;

public class HistogramSumMetricEntry : MetricEntry
{
    public int Count { get; set; }
    public double Sum { get; set; }
}

public class HistogramSumMetricEntryConfiguration : IEntityTypeConfiguration<HistogramSumMetricEntry>
{
    public void Configure(EntityTypeBuilder<HistogramSumMetricEntry> builder)
    {
        builder.ToTable("HistogramSumMetricEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Count).IsRequired();
        builder.Property(e => e.Sum).IsRequired();
        builder.HasOne(e => e.Metric)
            .WithMany(m => m.HistogramSumEntries)
            .HasForeignKey(e => e.MetricId);
        builder.Property(e => e.MetricId).IsRequired();
        builder.Property(e => e.TimeStampStartUtc).IsRequired();
        builder.Property(e => e.TimeStampEndUtc).IsRequired();
    }
}