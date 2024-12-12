using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace TemperatureDemoPlugin.Permissions;

public class TemperatureClaims
{
    public static List<string> ExportPermissions()
    {
        return
        [
            AllOnTemperature,
            ViewSensors,
            ChangeSettings
        ];
    }

    // ------------------ All ------------------
    public const string AllOnTemperature = "Temperature.*";
    public const string ViewSensors = "Temperature.Sensors.ViewSensors";
    public const string ChangeSettings = "Temperature.Sensors.ChangeSettings";

}