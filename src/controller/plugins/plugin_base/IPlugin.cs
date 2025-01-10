using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shared.Plugins;
using Shared.Plugins.DataInfo;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;

namespace PluginBase;

public interface IPlugin : IDisposable
{
    /// <summary>
    /// Gets the unique identifier for the plugin.
    /// This gets generated on Plugin Creation.
    /// Used to identify the Plugin in the Plugin Registry.
    /// </summary>
    Guid Guid { get; }

    /// <summary>
    /// Gets the formatted name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the author of the plugin.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the URL of the plugin's icon.
    /// </summary>
    string IconUrl { get; }

    /// <summary>
    /// Gets the URL of the plugin Page.
    /// </summary>
    string PluginUrl { get; }

    /// <summary>
    /// Initializes the Plugin. Might throw if the Plugin is not correctly configured.
    /// </summary>
    /// <param name="applicationServices">The WebApplication's ServiceProvider</param>
    /// <param name="pluginPath">The Plugin DLL Directory Path</param>
    /// <param name="hostPath">The Host Applications Root Directory</param>
    /// <exception cref="FileNotFoundException">Plugin Files incomplete</exception>
    /// <exception cref="FormatException">Invalid Plugin Files</exception>
    void OnInitialize(IServiceProvider applicationServices, string pluginPath, string hostPath);

    /// <summary>
    /// Gets called when the plugin is being loaded into the Request Pipeline.
    /// </summary>
    /// <param name="routeBuilder">The route builder to use for building endpoints</param>
    void OnEndpointBuilding(IEndpointRouteBuilder routeBuilder);

    /// <summary>
    /// Get called when the plugin services are being configured.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    void OnConfiguring(IServiceCollection services);

    /// <summary>
    /// Gets called when the plugin system successfully initialized and the Services were configured.
    /// </summary>
    /// <param name="services">The service provider to use for starting the system</param>
    void OnSystemStart(IServiceProvider services);

    public Task<DataResponseInfo> GetDataFromPlugin(DataRequestEntry request);

    public Task<DataInfoPluginResponse> GetPluginDataInfo();

    public Task RequestDataFromSensors();

}