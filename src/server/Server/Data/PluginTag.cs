using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Server.Data;

public class PluginTag
{
    public required string Tag { get; set; }
    public List<PluginEntry> Plugins { get; set; } = new();
}

public class PluginTagEntityConfiguration : IEntityTypeConfiguration<PluginTag>
{
    public void Configure(EntityTypeBuilder<PluginTag> builder)
    {
        builder.HasKey(e => e.Tag);
        builder.Property(e => e.Tag).IsRequired().HasMaxLength(30);
        builder.HasMany(t => t.Plugins)
            .WithMany(p => p.Tags)
            .UsingEntity<Dictionary<string, object>>(
                "PluginInfoTag",
                j => j.HasOne<PluginEntry>().WithMany().HasForeignKey("PluginEntryId"),
                j => j.HasOne<PluginTag>().WithMany().HasForeignKey("TagId")
            );
    }
}