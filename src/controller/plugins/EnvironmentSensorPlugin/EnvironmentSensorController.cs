using System.Net.Http.Json;
using PluginBase.Services.Data;
using PluginBase.Services.Devices;
using Shared;

namespace EnvironmentSensorPlugin;

public record DhtEntry(double Temperature, double Humidity);

public class EnvironmentSensorController(IHttpClientFactory clientFactory)
{
    public List<Device> Devices { get; set; }
    
    public void RegisterDevice(Device device)
    {
        Devices.Add(device);
    }
    
    public void UnregisterDevice(Device device)
    {
        Devices.Remove(device);
    }

    public async Task<DataQueryResult> ProcessTempQuery(DataQuery query)
    {
        var client = clientFactory.CreateClient();
        client.BaseAddress = new Uri("http://192.168.1.100");
        client.Timeout = TimeSpan.FromSeconds(10);
        var response = await client.GetAsync("/dht");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<DhtEntry>();
        
        return new ComboQueryResult(new Dictionary<string, object>()
        {
            {"Sensor 1", data!.Temperature} 
        });
    }
    
    public async Task<DataQueryResult> ProcessHumidityQuery(DataQuery query)
    {
        var client = clientFactory.CreateClient();
        client.BaseAddress = new Uri("http://192.168.1.100");
        client.Timeout = TimeSpan.FromSeconds(10);
        var response = await client.GetAsync("/dht");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<DhtEntry>();
        
        return new ComboQueryResult(new Dictionary<string, object>()
        {
            {"Sensor 1", data!.Humidity} 
        });
    }
}