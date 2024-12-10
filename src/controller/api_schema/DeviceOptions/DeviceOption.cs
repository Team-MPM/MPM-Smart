namespace ApiSchema;

public class DeviceOption
{
    public string Name { get; set; }
    public EType Type { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }
    public int DefaultValue { get; set; }
}


public enum EType {Radio, Slider, Checkbox}