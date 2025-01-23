using System.Text.Json;
using System.Text.Json.Serialization;

namespace PluginBase.Services.Devices;

public class DeviceTypeJsonConverter : JsonConverter<IDeviceType>
{
    public override void Write(Utf8JsonWriter writer, IDeviceType value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.GetType().Name);
        writer.WriteEndObject();
    }

    public override IDeviceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject");

        string? typeName = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected PropertyName");

            var propertyName = reader.GetString();
            reader.Read();

            if (propertyName == "Type")
            {
                typeName = reader.GetString();
            }
        }

        if (typeName == null)
            throw new JsonException("Type field is missing in JSON");

        var deviceType = ServiceProviderHelper.GetService<DeviceTypeRegistry>().GetRegisteredDevices()
            .FirstOrDefault(d => d.GetType().Name == typeName);

        if (deviceType == null)
            throw new JsonException($"Unknown device type: {typeName}");

        return deviceType;
    }
}