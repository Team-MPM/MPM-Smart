using System.Text.Json.Serialization;
using Shared;

namespace ApiSchema.Plugins;

[JsonConverter(typeof(PluginOptionsDtoConverter))]
public record PluginOptionsDto(
    OptionType Type,
    PluginOptionDetailsDto Details,
    string Name,
    bool Disabled = false);

public record PluginOptionValueDto(
    string Name,
    OptionType Type,
    object Value);

public record PluginOptionDetailsDto;

public record TextPluginOptionDetailsDto(
    string? Value = "",
    string? Placeholder = "",
    Range Lenght = default,
    TextOptionType TextType = TextOptionType.Text,
    string? Regex = null) : PluginOptionDetailsDto;

public record NumberPluginOptionDetailsDto(
    int? Value = 0,
    NumberOptionType NumberType = NumberOptionType.Number,
    int? Min = null,
    int? Max = null,
    int? Step = 1) : PluginOptionDetailsDto;

public record BooleanPluginOptionDetailsDto(
    bool Value = false) : PluginOptionDetailsDto;

public record SelectPluginOptionDetailsDto(
    string? Value,
    List<string?> Options,
    SelectOptionType Type) : PluginOptionDetailsDto;

public record MultiSelectPluginOptionDetailsDto(
    List<string> Values,
    List<string> Options,
    MultiSelectOptionType Type) : PluginOptionDetailsDto;

public record SubOptionsPluginOptionDetailsDto(
    List<PluginOptionsDto> SubOptions) : PluginOptionDetailsDto;