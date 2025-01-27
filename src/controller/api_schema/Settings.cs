namespace ApiSchema;

public record SettingsModel(string SystemName, string SystemTime, int TimeBetweenUpdatesInSec);
public record SystemNameModel(string SystemName);
public record SystemTimeModel(string TimeZoneCode);
public record TimeBetweenUpdatesModel(int TimeBetweenUpdatesSeconds);