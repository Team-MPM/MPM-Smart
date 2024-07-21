using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataModel.Primary;

public class AuditLogEntry
{
    public int Id { get; set; }
    public string Actor { get; set; }
    public string Action { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}

public class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.Actor).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Action).IsRequired().HasMaxLength(500);
        builder.Property(u => u.TimeStamp).IsRequired();
    }
}
