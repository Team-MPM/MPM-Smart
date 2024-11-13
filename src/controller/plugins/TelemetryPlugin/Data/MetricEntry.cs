namespace TelemetryPlugin.Data;

public class MetricEntry
{
    public int Id { get; set; }
    public int MetricId { get; set; }
    public Metric Metric { get; set; } = null!;
    public DateTime TimeStampStartUtc { get; set; }
    public DateTime TimeStampEndUtc { get; set; }
}