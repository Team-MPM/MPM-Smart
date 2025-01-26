using System.Text.Json.Serialization;
using Shared;

namespace ApiSchema;

[JsonConverter(typeof(OptionsConverter))]
public record OptionsDto(
    OptionType Type,
    OptionDetailsDto Details,
    string Name,
    bool Disabled = false);

public record OptionDetailsDto;

public record TextOptionDetailsDto(
    string? Value = "",
    string? Placeholder = "",
    Range Lenght = default,
    TextOptionType TextType = TextOptionType.Text,
    string? Regex = null) : OptionDetailsDto;

public record NumberOptionDetailsDto(
    int? Value = 0,
    NumberOptionType NumberType = NumberOptionType.Number,
    int? Min = null,
    int? Max = null,
    int? Step = 1) : OptionDetailsDto;

public record BooleanOptionDetailsDto(
    bool Value = false) : OptionDetailsDto;

public record SelectOptionDetailsDto(
    string? Value,
    List<string?> Options,
    SelectOptionType Type) : OptionDetailsDto;

public record MultiSelectOptionDetailsDto(
    List<string> Values,
    List<string> Options,
    MultiSelectOptionType Type) : OptionDetailsDto;
