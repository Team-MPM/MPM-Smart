using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TelemetryPlugin.Data;

public class HistogramBucketMetricEntry : MetricEntry
{
    public int Count { get; set; }
    public double Bucket { get; set; }
}

public class HistogramBucketMetricEntryConfiguration : IEntityTypeConfiguration<HistogramBucketMetricEntry>
{
    public void Configure(EntityTypeBuilder<HistogramBucketMetricEntry> builder)
    {
        builder.ToTable("HistogramBucketMetricEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Count).IsRequired();
        builder.Property(e => e.Bucket).IsRequired();
        builder.HasOne(e => e.Metric)
            .WithMany(m => m.HistogramBucketEntries)
            .HasForeignKey(e => e.MetricId);
        builder.Property(e => e.MetricId).IsRequired();
        builder.Property(e => e.TimeStampStartUtc).IsRequired();
        builder.Property(e => e.TimeStampEndUtc).IsRequired();
    }
}