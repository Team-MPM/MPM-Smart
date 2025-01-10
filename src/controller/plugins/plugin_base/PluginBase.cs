using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginBase.Services.Devices;
using PluginBase.Services.Options;

namespace PluginBase;

/// <summary>
/// Represents the base class for all plugins.
/// </summary>
/// <typeparam name="T">The type of the concrete plugin class.</typeparam>
public abstract class PluginBase<T> : IPlugin where T : PluginBase<T>, IDisposable
{
    public Guid Guid { get; } = Guid.NewGuid();
    public string Name { get; private set; } = null!;
    public string RegistryName { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public string Version { get; private set; } = null!;
    public string IconUrl => $"https://mpm-smart.g-martin.work/plugins/{RegistryName}/icon.png";
    public string PluginUrl => $"https://mpm-smart.g-martin.work/plugins/{RegistryName}";
    public OptionProvider Options => m_Options ?? throw new InvalidOperationException("Options not initialized");

    protected IServiceProvider ApplicationServices { get; private set; } = null!;
    protected IServiceProvider? Services { get; private set; }
    protected ILogger Logger { get; private set; } = null!;

    protected string PluginPath { get; private set; } = null!;
    protected string HostPath { get; private set; } = null!;

    private OptionProvider? m_Options;

    /// <summary>
    /// Initialize the plugin.
    /// </summary>
    protected abstract void Initialize();

    public void OnInitialize(IServiceProvider applicationServices, string pluginPath, string hostPath)
    {
        ApplicationServices = applicationServices;
        PluginPath = pluginPath;
        HostPath = hostPath;
        Logger = applicationServices.GetRequiredService<ILogger<T>>();

        var metadataPath = Path.Combine(PluginPath, "plugin.json");

        if (!File.Exists(metadataPath))
            throw new FileNotFoundException($"Metadata file not found at {metadataPath}");

        var json = File.ReadAllText(metadataPath);
        var metadata = JsonSerializer.Deserialize<PluginMetadata>(json);

        if (metadata == null)
            throw new FormatException("Failed to deserialize metadata file");

        Name = metadata.Name;
        RegistryName = metadata.RegistryName;
        Description = metadata.Description;
        Author = metadata.Author;
        Version = metadata.Version;

        Initialize();
    }

    /// <summary>
    /// Builds the endpoints for the plugin.
    /// </summary>
    /// <param name="routeBuilder">The route builder to use for building endpoints.</param>
    protected abstract void BuildEndpoints(IEndpointRouteBuilder routeBuilder);

    public virtual void OnEndpointBuilding(IEndpointRouteBuilder routeBuilder)
    {
        BuildEndpoints(routeBuilder);
    }

    /// <summary>
    /// Configures the services for the plugin.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    protected abstract void ConfigureServices(IServiceCollection services);

    public virtual void OnConfiguring(IServiceCollection services)
    {
        ConfigureServices(services);
    }

    /// <summary>
    /// Starts the system for the plugin. Services and Options are available.
    /// </summary>
    protected abstract void SystemStart();

    public void OnSystemStart(IServiceProvider services)
    {
        Services = services;
        var optionBuilder = new OptionsBuilder(RegistryName);
        OnOptionBuilding(optionBuilder);
        m_Options = optionBuilder.Build();
        m_Options.Load().Wait();
        SystemStart();
    }

    /// <summary>
    /// Specify and map the options for the plugin.
    /// Services are already configured and built here.
    /// </summary>
    protected abstract void OnOptionBuilding(OptionsBuilder builder);

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}