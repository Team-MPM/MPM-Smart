using System.Text.Json;
using System.Text.Json.Serialization;
using Shared;

namespace ApiSchema.Plugins;

public class PluginOptionsDtoConverter : JsonConverter<PluginOptionsDto>
{
    public override PluginOptionsDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = (OptionType)root.GetProperty("Type").GetInt32();
        var name = root.GetProperty("Name").GetString()!;
        var disabled = root.GetProperty("Disabled").GetBoolean();
        PluginOptionDetailsDto details = type switch
        {
            OptionType.Text => JsonSerializer.Deserialize<TextPluginOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Number => JsonSerializer.Deserialize<NumberPluginOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Boolean => JsonSerializer.Deserialize<BooleanPluginOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Select => JsonSerializer.Deserialize<SelectPluginOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.MultiSelect => JsonSerializer.Deserialize<MultiSelectPluginOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            _ => throw new ArgumentException("Unsupported type")
        };

        return new PluginOptionsDto(type, details, name, disabled);
    }

    public override void Write(Utf8JsonWriter writer, PluginOptionsDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Type", (int)value.Type);
        writer.WriteString("Name", value.Name);
        writer.WriteBoolean("Disabled", value.Disabled);
        writer.WritePropertyName("Details");
        JsonSerializer.Serialize(writer, value.Details, value.Details.GetType(), options);
        writer.WriteEndObject();
    }
}