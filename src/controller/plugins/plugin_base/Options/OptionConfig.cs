namespace PluginBase.Options;

public enum OptionUiElement
{
    Input,
    Password,
    Email,
    Address,
    Textarea,
    Select,
    Checkbox,
    Radio,
    Slider,
    Dropdown,
    Switch,
    Number,
    Tags
}

public class OptionConfig
{
    public required Type Type { get; init; }
    public required string Name { get; init; }
    public required OptionUiElement UiElement { get; set; }
    public object? DefaultValue { get; set; }
    public object? Placeholder { get; set; }
    public string? Regex { get; set; }
    public Range? Range { get; set; }
    public List<object>? AllowedValues { get; set; }
    public bool MultiSelect { get; set; }
    public Action<OptionConfig>? LoadCallback { get; init; }

    public void Update() => LoadCallback?.Invoke(this);
}