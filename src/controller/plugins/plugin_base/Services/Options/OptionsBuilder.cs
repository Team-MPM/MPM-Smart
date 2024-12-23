using System.Collections;

namespace PluginBase.Services.Options;

public class OptionsBuilder(string pluginName)
{
    private readonly Dictionary<string, OptionConfig> m_Configs = new();

    public void Option<T>(string name, OptionUiElement? element = null, Action<OptionConfig>? config = null)
    {
        var exists = m_Configs.TryGetValue(name, out var existingOption);
        var option = exists
            ? existingOption
            : new OptionConfig
            {
                Name = name,
                Type = typeof(T),
                UiElement = element ?? GetDefaultUiElement<T>()
            };
        config?.Invoke(option!);
        m_Configs[name] = option!;
    }

    private OptionUiElement GetDefaultUiElement<T>() =>
        typeof(T) switch
        {
            { } t when t == typeof(bool) => OptionUiElement.Switch,
            { } t when t == typeof(int) => OptionUiElement.Number,
            { } t when t == typeof(long) => OptionUiElement.Number,
            { } t when t == typeof(short) => OptionUiElement.Number,
            { } t when t == typeof(byte) => OptionUiElement.Number,
            { } t when t == typeof(sbyte) => OptionUiElement.Number,
            { } t when t == typeof(ushort) => OptionUiElement.Number,
            { } t when t == typeof(uint) => OptionUiElement.Number,
            { } t when t == typeof(ulong) => OptionUiElement.Number,
            { } t when t == typeof(float) => OptionUiElement.Number,
            { } t when t == typeof(double) => OptionUiElement.Number,
            { } t when t == typeof(decimal) => OptionUiElement.Number,
            { } t when t == typeof(string) => OptionUiElement.Input,
            { } t when t == typeof(List<string>) => OptionUiElement.Dropdown,
            _ => throw new ArgumentException("Unsupported type")
        };

    public OptionProvider Build()
    {
        foreach (var pair in m_Configs)
        {
            var option = pair.Value;
            var ui = option.UiElement;
            var type = option.Type;
            var isText = type == typeof(string);
            var isNumber = type == typeof(int) || type == typeof(long) || type == typeof(short) ||
                           type == typeof(byte) ||
                           type == typeof(sbyte) || type == typeof(ushort) || type == typeof(uint) ||
                           type == typeof(ulong) ||
                           type == typeof(float) || type == typeof(double) || type == typeof(decimal);
            var isBool = type == typeof(bool);
            var isList = type.GetInterface(nameof(ICollection)) != null;

            if (isText)
            {
                if (ui is not (OptionUiElement.Input or OptionUiElement.Email or OptionUiElement.Address
                    or OptionUiElement.Password or OptionUiElement.Textarea or OptionUiElement.Select
                    or OptionUiElement.Dropdown or OptionUiElement.Radio))
                    throw new ArgumentException(
                        "Only input, email, address, password, dropdown, radio,  select and textarea are supported for text");
                if (option.AllowedValues is not null && ui is OptionUiElement.Select)
                    throw new ArgumentException("Allowed values are not supported for text. Use Regex!");
            }
            else if (isNumber)
            {
                if (ui is not (OptionUiElement.Number or OptionUiElement.Slider))
                    throw new ArgumentException("Only number is supported for numbers");
                if (option.Range is null && ui is OptionUiElement.Slider)
                    throw new ArgumentException("Range must be provided for numbers");
                if (option.AllowedValues is not null || option.Placeholder is not null || option.Regex is not null)
                    throw new ArgumentException("Allowed values, placeholder, and regex are not supported for numbers");
            }
            else if (isBool)
            {
                if (ui is not OptionUiElement.Switch)
                    throw new ArgumentException("Only switch is supported for bool");
                if (option.AllowedValues is not null || option.Placeholder is not null || option.Regex is not null ||
                    option.Range is not null)
                    throw new ArgumentException(
                        "Allowed values, placeholder, range and regex are not supported for bool");
            }
            else if (isList)
            {
                if (option.AllowedValues is null || option.AllowedValues.Count == 0)
                    throw new ArgumentException("Allowed values must be provided for lists");
                if (ui is not (OptionUiElement.Dropdown or OptionUiElement.Tags))
                    throw new ArgumentException("Only dropdown and tags are supported for lists");
                if (option.Placeholder is not null || option.Regex is not null || option.Range is not null)
                    throw new ArgumentException("Placeholder, range and regex are not supported for bool");
            }
            else
            {
                throw new ArgumentException("Unsupported type");
            }
        }

        return new OptionProvider($"{pluginName}.options")
        {
            OptionConfigs = m_Configs.AsReadOnly()
        };
    }
}