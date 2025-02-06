using System.Text.Json;
using System.Text.Json.Serialization;
using Shared;

namespace PluginBase.Services.Data;


public class DataIndex
{
    public Dictionary<Guid, DataPoint> Entries { get; } = new();

    private readonly Dictionary<ReverseLookupKey, Guid> m_ReverseLookup;
    private const string ReverseLookupPath = "DataIndexReverseLookup.json";

    private readonly JsonSerializerOptions m_JsonOptions = new()
    {
        Converters = { new ReverseLookupKeyConverter() }
    };
    
    private record struct ReverseLookupKey(string DataPointName, string PluginName, string Unit, DataQueryType Type);
    
    public DataIndex()
    {
        if (File.Exists(ReverseLookupPath))
        {
            var json = File.ReadAllText(ReverseLookupPath);
            m_ReverseLookup = JsonSerializer.Deserialize<Dictionary<ReverseLookupKey, Guid>>(json, m_JsonOptions)!;
        }
        else
        {
            m_ReverseLookup = new Dictionary<ReverseLookupKey, Guid>();
        }
    }

    public DataPoint Add(DataPoint entry)
    {
        ValidateDataPoint(entry);

        var key = new ReverseLookupKey(entry.Name, entry.Plugin.Name, entry.Unit, entry.QueryType);
        if (m_ReverseLookup.TryGetValue(key, out var value))
            entry.Id = value;
        else
        {
            m_ReverseLookup.Add(key, entry.Id);
            PersistReverseMap();
        }
            
        if (!Entries.TryAdd(entry.Id, entry))
            throw new InvalidOperationException("DataPoint with the same signature is already registered");

        return entry;
    }

    private static void ValidateDataPoint(DataPoint entry)
    {
        if (DataTypeHelper.IsCombo(entry.QueryType))
        {
            if (entry.ComboOptions is null && entry.ComboOptionsSource is null)
                throw new InvalidOperationException(
                    "Either ComboOptions or ComboOptionsSource have to be set when combo type is specified");

            if (entry.ComboOptions is not null && entry.ComboOptionsSource is not null)
                throw new InvalidOperationException(
                    "ComboOptions and ComboOptionsSource are mutually exclusive");
        }

        if (!DataTypeHelper.IsSeries(entry.QueryType)) return;
        
        if (entry.AvailableGranularity is null)
            throw new InvalidOperationException("AvailableGranularity must be set for type series");
    }

    private void PersistReverseMap()
    {
        var json = JsonSerializer.Serialize(m_ReverseLookup, m_JsonOptions);
        File.Delete(ReverseLookupPath);
        File.WriteAllText(ReverseLookupPath, json);
    }
    
    
    private class ReverseLookupKeyConverter : JsonConverter<ReverseLookupKey>
    {
        public override ReverseLookupKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            var parts = value!.Split('|', StringSplitOptions.None);
            if (parts.Length != 4)
            {
                throw new JsonException("Invalid format for ReverseLookupKey.");
            }

            return new ReverseLookupKey(parts[0], parts[1], parts[2], Enum.Parse<DataQueryType>(parts[3]));
        }

        public override void Write(Utf8JsonWriter writer, ReverseLookupKey value, JsonSerializerOptions options)
        {
            var serializedKey = $"{value.DataPointName}|{value.PluginName}|{value.Unit}|{value.Type}";
            writer.WriteStringValue(serializedKey);
        }
        
        public override ReverseLookupKey ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var key = reader.GetString();
            var parts = key!.Split('|', StringSplitOptions.None);
            if (parts.Length != 4)
            {
                throw new JsonException("Invalid format for ReverseLookupKey.");
            }

            return new ReverseLookupKey(parts[0], parts[1], parts[2], Enum.Parse<DataQueryType>(parts[3]));
        }

        public override void WriteAsPropertyName(Utf8JsonWriter writer, ReverseLookupKey value, JsonSerializerOptions options)
        {
            var serializedKey = $"{value.DataPointName}|{value.PluginName}|{value.Unit}|{value.Type}";
            writer.WritePropertyName(serializedKey);
        }
    }
}