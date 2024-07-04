namespace VirtualSystem.Networking;

public class Connectivity : SystemBase.Networking.Connectivity
{
    public override bool Init()
    {
        EthernetAdapters.Add(new EthernetAdapter()
        {
            Connected = true
        });
        
        return true;
    }

    public override bool Refresh()
    {
        return true;
    }
}