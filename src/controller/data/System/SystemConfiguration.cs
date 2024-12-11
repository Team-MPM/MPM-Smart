using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.System;

public class SystemConfiguration
{
    public int Id { get; set; }
    public string SystemName { get; set; } = "Controller";
    public TimeZoneCode TimeZoneCode { get; set; } = TimeZoneCode.UTC;
    public int TimeBetweenDataUpdatesSeconds { get; set; } = 5;
}

public class SystemSettingsEntityConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.Property(s => s.TimeZoneCode).HasConversion<string>();
        builder.HasKey(s => s.Id);
    }
}