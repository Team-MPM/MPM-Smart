using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SystemBase;

namespace SmartHomeHost;

public static class SmartHome
{
    public static WebApplicationBuilder CreateBuilder<TSystem>(string[] args) where TSystem : class, ISystem
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddSingleton<ISystem, TSystem>();
        return builder;
    }
}