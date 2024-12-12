using TemperatureDemoPlugin.Data;

namespace TemperatureDemoPlugin.Endpoints;

public class TemperatureSensorController
{
    private readonly TemperatureDemoContext _context;

    public TemperatureSensorController(TemperatureDemoContext context)
    {
        _context = context;
    }

    public async Task<List<Sensor>> GetSensors()
    {
        throw new NotImplementedException();
    }
}