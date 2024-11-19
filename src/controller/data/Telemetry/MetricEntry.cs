using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Telemetry;

public class MetricEntry
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string MeterName { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Unit { get; set; } = null!;
}

public class MetricEntryEntity : MetricEntry
{
    public int Id { get; set; }
}

public class MetricEntryEntityConfiguration : IEntityTypeConfiguration<MetricEntryEntity>
{
    public void Configure(EntityTypeBuilder<MetricEntryEntity> builder)
    {
        builder.ToTable("MetricEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(e => e.Description)
            .IsRequired();
        builder.Property(e => e.MeterName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(e => e.Unit)
            .HasMaxLength(20)
            .IsRequired();
    }
}