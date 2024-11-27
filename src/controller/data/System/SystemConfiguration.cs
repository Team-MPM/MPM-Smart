using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.System;

public class SystemConfiguration
{
    public int Id { get; set; }
    public string SystemName { get; set; } = "Controller";
    public TimeZones TimeZone { get; set; } = TimeZones.UTC;
    public int TimeBetweenDataUpdatesSeconds { get; set; } = 5;
}

public enum TimeZones // TODO add more time zones
{
    UTC = 0,
    CET = 1,
    EET = 2,
    EST = -5,
    CST = -6,
    MST = -7,
    PST = -8,
    GMT = 0,
    IST = 5,
    JST = 9,
    AEST = 10
}

public class SystemSettingsEntityConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.Property(s => s.TimeZone).HasConversion<string>();
        builder.HasKey(s => s.Id);
    }
}