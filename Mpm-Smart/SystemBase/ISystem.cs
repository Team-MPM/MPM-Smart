namespace SystemBase;

public enum SystemState
{
    Ready,
    Running,
    Stopped
}

public interface ISystem
{
    public SystemState State { get; set; }
    public Networking.Connectivity Connectivity { get; set; }
}