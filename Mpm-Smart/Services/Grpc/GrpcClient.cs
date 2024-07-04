using Grpc.Net.Client;

namespace Services.Grpc;

public class GrpcClientOptions
{
    public Uri Host { get; set; } = null!;
    // TODO
}

public abstract class GrpcClient
{
    public GrpcClientOptions Options { get; } = new();

    protected GrpcChannel Channel { get; private set; } = null!;

    public void Init()
    {
        Channel = GrpcChannel.ForAddress(Options.Host.ToString());
    }
}