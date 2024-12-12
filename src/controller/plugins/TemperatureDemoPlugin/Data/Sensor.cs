using System.ComponentModel.DataAnnotations.Schema;

namespace TemperatureDemoPlugin.Data;

[Table("Sensors")]
public class Sensor
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string Name { get; set; }
    public required string Token { get; set; }
    public required DateTime LastUpdateDate { get; set; }

    public List<DataEntry> DataEntries { get; set; } = new();
}