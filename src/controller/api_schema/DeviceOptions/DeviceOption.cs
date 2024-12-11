namespace ApiSchema;

public class DeviceOption
{
    public string Name { get; set; }
    public EDeviceOptionType Type { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }
    public int DefaultValue { get; set; }
}

public enum EDeviceOptionType {Radio, Slider, Checkbox}