﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModuleBase;
using PluginBase;
using SystemBase;
using SystemBase.Exceptions;

namespace SmartHomeHost;

public class SmartHomeBuilder(WebApplicationBuilder builder) : IHostApplicationBuilder
{
    public IConfigurationManager Configuration => builder.Configuration;
    public IHostEnvironment Environment => builder.Environment;
    public ILoggingBuilder Logging => builder.Logging;
    public IMetricsBuilder Metrics => builder.Metrics;
    public IDictionary<object, object> Properties => ((IHostApplicationBuilder)builder).Properties;
    public IServiceCollection Services => builder.Services;

    public List<IModule> Modules { get; set; } = [];

    public static SmartHomeBuilder Create<TSystem>(string[] args) where TSystem : class, ISystem
    {
        var builder = WebApplication.CreateSlimBuilder(args);
        builder.Services.AddSingleton(builder.Services);
        builder.Services.AddMvc();
        
        builder.Services.AddSingleton<ISystem, TSystem>();
        
        builder.Services.AddSingleton<ModuleManager>();
        var moduleLoader = new ModuleLoader(builder.Configuration);
        moduleLoader.LoadAssemblies();
        var modules = moduleLoader.LoadModules();
        
        foreach (var module in modules)
        {
            builder.Services.AddSingleton(module.GetType(), module);
        }
        
        builder.Services.AddSingleton<PluginManager>();
        
        return new SmartHomeBuilder(builder);
    }

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null) where TContainerBuilder : notnull
    {
        throw new NotImplementedException();
    }
    
    public WebApplication Build()
    {
        var app = builder.Build();
        
        var logger = app.Services.GetRequiredService<ILogger<SmartHomeBuilder>>();

        var system = app.Services.GetRequiredService<ISystem>();
        if (system.State != SystemState.Running)
            throw new SystemFailureException("System failed to start");

        var moduleManager = app.Services.GetRequiredService<ModuleManager>();
        moduleManager.StartModules();
        
        var pluginManager = app.Services.GetRequiredService<PluginManager>();
        pluginManager.Update();

        app.UseMiddleware<PluginMiddleware>();
        
        return app;
    }
}