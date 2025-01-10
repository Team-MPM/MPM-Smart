namespace Shared;

public enum OptionType
{
    Text,
    Number,
    Boolean,
    Select,
    MultiSelect
}

public enum TextOptionType
{
    Text,
    Password,
    Email,
    Url,
    Address
}

public enum NumberOptionType
{
    Number,
    Slider
}

public enum SelectOptionType
{
    Dropdown,
    Radio,
}

public enum MultiSelectOptionType
{
    Dropdown,
    Checkbox,
    Tags
}