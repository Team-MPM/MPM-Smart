namespace TelemetryPlugin.Data;

public class Metric
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string MetricName { get; set; }
    public string Description { get; set; }
    public string Unit { get; set; }
    public MetricType Type { get; set; }
    public ICollection<MetricEntry> Entries { get; set; }
}