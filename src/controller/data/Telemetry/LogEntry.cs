using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Telemetry;

public class LogEntry
{
    public DateTime TimeStampUtc { get; set; }
    public string? TraceId { get; set; }
    public string? Category { get; set; }
    public string LogLevel { get; set; } = null!;
    public string Message { get; set; } = null!;
}

public class LogEntryEntity : LogEntry
{
    public int Id { get; set; }
}

public class LogEntryEntityConfiguration : IEntityTypeConfiguration<LogEntryEntity>
{
    public void Configure(EntityTypeBuilder<LogEntryEntity> builder)
    {
        builder.ToTable("LogEntries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TraceId)
            .IsFixedLength()
            .HasMaxLength(16);
        builder.Property(e => e.LogLevel)
            .IsRequired()
            .HasMaxLength(16);
        builder.Property(e => e.Category)
            .HasMaxLength(50);
        builder.Property(e => e.Message)
            .IsRequired()
            .HasMaxLength(1000);
    }
}