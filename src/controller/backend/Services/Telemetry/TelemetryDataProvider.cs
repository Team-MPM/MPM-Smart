using ApiSchema.Telemetry;

namespace Backend.Services.Telemetry;

public class TelemetryDataProvider
{
    public IEnumerable<LogEntry> GetLogEntries()
    {
        throw new NotImplementedException();
    }
    
    public IEnumerable<MetricsEntry> GetMetricsEntries()
    {
        throw new NotImplementedException();
    }
    
    public IEnumerable<TraceEntry> GetTraceEntries()
    {
        throw new NotImplementedException();
    }
}