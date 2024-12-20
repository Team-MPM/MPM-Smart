namespace ApiSchema.Settings;

public class SettingsModel
{
    public required string SystemName { get; set; }
    public required string SystemTime { get; set; }
    public required int TimeBetweenUpdatesInSec { get; set; }
}