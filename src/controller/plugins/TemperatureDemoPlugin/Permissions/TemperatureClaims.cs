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
        ];
    }

    // ------------------ All ------------------
    public const string AllOnTemperature = "Temperature.*";

    // ------------------ Sensor ------------------
    public const string ViewSensors = "Temperature.Sensors.ViewSensors";
    public const string ViewSensorData = "Temperature.Sensors.ViewSensorData";
    public const string ModifySensorData = "Temperature.Sensors.ModifySensorData";

    // ------------------ Management ------------------
    public const string AllOnManagement = "Temperature.Management.*";
    public const string AddSensors = "Temperature.Management.AddSensors";
    public const string ModifySensors = "Temperature.Management.ChangeSettings";
    public const string DeleteSensors = "Temperature.Management.DeleteSensors";


}