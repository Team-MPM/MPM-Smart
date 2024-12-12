using System.ComponentModel.DataAnnotations.Schema;

namespace TemperatureDemoPlugin.Data;

[Table("DataEntries")]
public class DataEntry
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required int SensorId { get; set; }
    public required Sensor Sensor { get; set; }

    public required DateTime CaptureTime { get; set; }

    public double? TemperatureC { get; set; }
    public double? HumidityPercent { get; set; }
}