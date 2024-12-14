﻿public enum TimeZoneCode
{
    UTC,
    CET,
    EET,
    EST,
    CST,
    MST,
    PST,
    GMT,
    IST,
    JST,
    AEST,
    AEDT,
    ACST,
    AKST,
    ART,
    AST,
    AWST,
    BRT,
    CAT,
    CCT,
    CDT,
    CEST,
    CLT,
    CST6CDT,
    EAT,
    ECT,
    EDT,
    HKT,
    HST,
    IRKT,
    KST,
    MDT,
    MSK,
    NZST,
    PKT,
    PHT,
    SAST,
    SGT,
    UTC12,
    WAT,
    WET
}

public static class TimeZoneList
{
    public static Dictionary<TimeZoneCode, double> TimeZones = new()
    {
        { TimeZoneCode.UTC, 0 },
        { TimeZoneCode.CET, 1 },
        { TimeZoneCode.EET, 2 },
        { TimeZoneCode.EST, -5 },
        { TimeZoneCode.CST, -6 },
        { TimeZoneCode.MST, -7 },
        { TimeZoneCode.PST, -8 },
        { TimeZoneCode.GMT, 0 },
        { TimeZoneCode.IST, 5 },
        { TimeZoneCode.JST, 9 },
        { TimeZoneCode.AEST, 10 },
        { TimeZoneCode.AEDT, 11 },
        { TimeZoneCode.ACST, 9.5 },
        { TimeZoneCode.AKST, -9 },
        { TimeZoneCode.ART, -3 },
        { TimeZoneCode.AST, -4 },
        { TimeZoneCode.AWST, 8 },
        { TimeZoneCode.BRT, -3 },
        { TimeZoneCode.CAT, 2 },
        { TimeZoneCode.CCT, 6.5 },
        { TimeZoneCode.CDT, -5 },
        { TimeZoneCode.CEST, 2 },
        { TimeZoneCode.CLT, -4 },
        { TimeZoneCode.CST6CDT, -6 },
        { TimeZoneCode.EAT, 3 },
        { TimeZoneCode.ECT, -5 },
        { TimeZoneCode.EDT, -4 },
        { TimeZoneCode.HKT, 8 },
        { TimeZoneCode.HST, -10 },
        { TimeZoneCode.IRKT, 8 },
        { TimeZoneCode.KST, 9 },
        { TimeZoneCode.MDT, -6 },
        { TimeZoneCode.MSK, 3 },
        { TimeZoneCode.NZST, 12 },
        { TimeZoneCode.PKT, 5 },
        { TimeZoneCode.PHT, 8 },
        { TimeZoneCode.SAST, 2 },
        { TimeZoneCode.SGT, 8 },
        { TimeZoneCode.UTC12, 12 },
        { TimeZoneCode.WAT, 1 },
        { TimeZoneCode.WET, 0 }
    };
}