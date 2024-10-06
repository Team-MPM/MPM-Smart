using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace System.Data;

public record UserProfile
{
    public bool UseDarkMode { get; set; } = false;
}

public record UserProfileEntity : UserProfile
{
    public int Id { get; set; }
}

public class UserProfileEntityConfiguration : IEntityTypeConfiguration<UserProfileEntity>
{
    public void Configure(EntityTypeBuilder<UserProfileEntity> builder)
    {
        builder.ToTable("UserProfile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UseDarkMode).IsRequired();
    }
}