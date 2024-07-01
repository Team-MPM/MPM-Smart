using Microsoft.Extensions.DependencyInjection;

namespace Services.Grpc;

public static class GrpcExtensions
{
    public delegate void OptionsBuilder(GrpcClientOptions options);
    
    public static void AddGrpcClient<T>(this IServiceCollection services, OptionsBuilder optionsBuilder)
        where T : GrpcClient, new()
    {
        var client = new T();
        optionsBuilder(client.Options);
        client.Init();
        services.AddSingleton<T>(client);
    }
}