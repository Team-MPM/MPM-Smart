using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Server.Data;

public class PluginEntry()
{
    public required Guid Id { get; set; }
    public required string Name { get; init; }
    public required string RegistryName { get; init; }
    public required List<PluginTag> Tags { get; init; }
    public required ServerUser Author { get; init; }
}

public class PluginInfoEntityConfiguration : IEntityTypeConfiguration<PluginEntry>
{
    public void Configure(EntityTypeBuilder<PluginEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RegistryName).IsRequired().HasMaxLength(50);
        builder.HasOne(e => e.Author).WithMany(a => a.Plugins).IsRequired();
    }
}