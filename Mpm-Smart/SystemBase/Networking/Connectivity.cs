namespace SystemBase.Networking;

public abstract class Connectivity
{
    public List<AEthernetAdapter> EthernetAdapters { get; set; } = [];
    public List<AWifiAdapter> WifiAdapters { get; set; } = [];

    public abstract bool Init();
    public abstract bool Refresh();
}