namespace ApiSchema.Telemetry;

public class TraceEntry : ICsvSerializable
{
    public string ToCsv()
    {
        return "test";
    }
}