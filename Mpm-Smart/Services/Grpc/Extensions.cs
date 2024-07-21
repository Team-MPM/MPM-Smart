using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services.Grpc;

public static class Extensions
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

    public static IHostApplicationBuilder AddGrpc(this IHostApplicationBuilder builder)
    {
        builder.Services.AddGrpc();
        return builder;
    }
}