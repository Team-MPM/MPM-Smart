using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.System;

public class SystemConfiguration
{
    public int Id { get; set; }
    public string SystemName { get; set; } = "Controller";
    public string TimeZone { get; set; } = TimeZoneList.TimeZones.Select(s => s.Code).First(s => s == "UTC");
    public int TimeBetweenDataUpdatesSeconds { get; set; } = 5;
}

public class TimeZone()
{
    public string Code { get; set; }
    public double DifferenceToUTC { get; set; }
}

public static class TimeZoneList
{
    public static List<TimeZone> TimeZones = new List<TimeZone>()
    {
        new TimeZone() { Code = "UTC", DifferenceToUTC = 0 },
        new TimeZone() { Code = "CET", DifferenceToUTC = 1 },
        new TimeZone() { Code = "EET", DifferenceToUTC = 2 },
        new TimeZone() { Code = "EST", DifferenceToUTC = -5 },
        new TimeZone() { Code = "CST", DifferenceToUTC = -6 },
        new TimeZone() { Code = "MST", DifferenceToUTC = -7 },
        new TimeZone() { Code = "PST", DifferenceToUTC = -8 },
        new TimeZone() { Code = "GMT", DifferenceToUTC = 0 },
        new TimeZone() { Code = "IST", DifferenceToUTC = 5 },
        new TimeZone() { Code = "JST", DifferenceToUTC = 9 },
        new TimeZone() { Code = "AEST", DifferenceToUTC = 10 },
        new TimeZone() { Code = "AEDT", DifferenceToUTC = 11 },
        new TimeZone() { Code = "ACST", DifferenceToUTC = 9.5 },
        new TimeZone() { Code = "AKST", DifferenceToUTC = -9 },
        new TimeZone() { Code = "ART", DifferenceToUTC = -3 },
        new TimeZone() { Code = "AST", DifferenceToUTC = -4 },
        new TimeZone() { Code = "AWST", DifferenceToUTC = 8 },
        new TimeZone() { Code = "BRT", DifferenceToUTC = -3 },
        new TimeZone() { Code = "CAT", DifferenceToUTC = 2 },
        new TimeZone() { Code = "CCT", DifferenceToUTC = 6.5 },
        new TimeZone() { Code = "CDT", DifferenceToUTC = -5 },
        new TimeZone() { Code = "CEST", DifferenceToUTC = 2 },
        new TimeZone() { Code = "CLT", DifferenceToUTC = -4 },
        new TimeZone() { Code = "CST6CDT", DifferenceToUTC = -6 },
        new TimeZone() { Code = "EAT", DifferenceToUTC = 3 },
        new TimeZone() { Code = "ECT", DifferenceToUTC = -5 },
        new TimeZone() { Code = "EDT", DifferenceToUTC = -4 },
        new TimeZone() { Code = "HKT", DifferenceToUTC = 8 },
        new TimeZone() { Code = "HST", DifferenceToUTC = -10 },
        new TimeZone() { Code = "IRKT", DifferenceToUTC = 8 },
        new TimeZone() { Code = "KST", DifferenceToUTC = 9 },
        new TimeZone() { Code = "MDT", DifferenceToUTC = -6 },
        new TimeZone() { Code = "MSK", DifferenceToUTC = 3 },
        new TimeZone() { Code = "NZST", DifferenceToUTC = 12 },
        new TimeZone() { Code = "PKT", DifferenceToUTC = 5 },
        new TimeZone() { Code = "PHT", DifferenceToUTC = 8 },
        new TimeZone() { Code = "SAST", DifferenceToUTC = 2 },
        new TimeZone() { Code = "SGT", DifferenceToUTC = 8 },
        new TimeZone() { Code = "UTC12", DifferenceToUTC = 12 },
        new TimeZone() { Code = "WAT", DifferenceToUTC = 1 },
        new TimeZone() { Code = "WET", DifferenceToUTC = 0 }
    };
}

public class SystemSettingsEntityConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.Property(s => s.TimeZone);
        builder.HasKey(s => s.Id);
    }
}