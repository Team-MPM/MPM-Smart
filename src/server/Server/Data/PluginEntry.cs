using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Server.Data;

public class PluginEntry()
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required string RegistryName { get; init; }
    public required string Description { get; init; }
    public List<PluginTag> Tags { get; init; } = null!;
    public required string AuthorId { get; set; }
    public ServerUser Author { get; init; } = null!;
}

public class PluginInfoEntityConfiguration : IEntityTypeConfiguration<PluginEntry>
{
    public void Configure(EntityTypeBuilder<PluginEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RegistryName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(1000);
        builder
            .HasOne(e => e.Author)
            .WithMany(a => a.Plugins)
            .HasForeignKey(e => e.AuthorId)
            .IsRequired();
    }
}