namespace Shared.Services.Sensors.TempDemo;

public class AddDemoTempEntry
{
    public required string Token { get; set; }
    public required double Humidity { get; set; }
    public required double Temperature { get; set; }
}