namespace SystemBase;

public abstract class SystemBase(Networking.Connectivity connectivity) : ISystem
{
    public SystemState State { get; set; } = SystemState.Ready;
    public Networking.Connectivity Connectivity { get; set; } = connectivity;
}