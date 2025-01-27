using System.Text.Json;
using System.Text.Json.Serialization;
using Shared;

namespace ApiSchema;

public class OptionsConverter : JsonConverter<OptionsDto>
{
    public override OptionsDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var type = (OptionType)root.GetProperty("Type").GetInt32();
        var name = root.GetProperty("Name").GetString()!;
        var disabled = root.GetProperty("Disabled").GetBoolean();
        OptionDetailsDto details = type switch
        {
            OptionType.Text => JsonSerializer.Deserialize<TextOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Number => JsonSerializer.Deserialize<NumberOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Boolean => JsonSerializer.Deserialize<BooleanOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.Select => JsonSerializer.Deserialize<SelectOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            OptionType.MultiSelect => JsonSerializer.Deserialize<MultiSelectOptionDetailsDto>(root.GetProperty("Details").GetRawText(), options)!,
            _ => throw new ArgumentException("Unsupported type")
        };

        return new OptionsDto(type, details, name, disabled);
    }

    public override void Write(Utf8JsonWriter writer, OptionsDto value, JsonSerializerOptions options)
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