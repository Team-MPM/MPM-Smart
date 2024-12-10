using System.Text.Json.Serialization;

namespace ApiSchema.Settings;

public class TimeBetweenUpdatesModel
{
    [JsonPropertyName("timebetweenupdatesseconds")]
    public int TimeBetweenUpdatesSeconds{ get; set; }
}