using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Backend.Services.Plugins;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace PluginTests.Fixtures;

public class PluginFixture
{
    public Mock<IPluginLoader> MockPluginLoader { get; private set; } = null!;
    public Mock<IPluginManager> MockPluginManager { get; private set; } = null!;
    public Mock<IWebHostEnvironment> MockWebHostEnvironment { get; private set; } = null!;
    public Mock<IServiceProvider> MockServiceProvider { get; private set; } = null!;
    public Mock<ILogger<PluginManager>> MockPluginManagerLogger { get; private set; } = null!;
    public Mock<ILogger<PluginLoader>> MockPluginLoaderLogger { get; private set; } = null!;

    public ServiceProvider ServiceProvider { get; private set; } = null!;
    public MockFileSystem MockFileSystem { get; private set; } = null!;

    public int PluginCount { get; private set; }
    public string HostPath { get; private set; }

    public const string TestPluginName = "Test Plugin";
    public const string TestPluginRegistryName = "test-plugin";
    public const string TestPluginDescription = "A test plugin";
    public const string TestPluginAuthor = "Test Author";
    public const string TestPluginVersion = "1.0.0";

    public const string TestPluginJson = $$"""
                                            {
                                              "Name": "{{TestPluginName}}",
                                              "RegistryName": "{{TestPluginRegistryName}}",
                                              "Description": "{{TestPluginDescription}}",
                                              "Author": "{{TestPluginAuthor}}",
                                              "Version": "{{TestPluginVersion}}"
                                            }
                                            """;

    private readonly byte[] m_TestPluginAssemblyBin;
    
    public PluginFixture()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var pluginSourceDir = Path.Combine(assemblyLocation,
            "../../../../../../../build/plugins");
        HostPath = Path.Combine(assemblyLocation, "..", "app", "controller", "backend");

        m_TestPluginAssemblyBin = File.ReadAllBytes(pluginSourceDir + "/test-plugin/TestPlugin.dll");

        PluginCount = 1;
    }

    public void Setup()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        
        MockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            {
                Path.Combine(assemblyLocation, "..", "app", "plugins", "test", "plugin.json"),
                new MockFileData(TestPluginJson)
            },
            {
                Path.Combine(assemblyLocation, "..", "app", "plugins", "test", "TestPlugin.dll"),
                new MockFileData(m_TestPluginAssemblyBin)
            }
        }, HostPath);
        
        var services = new ServiceCollection();

        MockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        MockPluginLoader = new Mock<IPluginLoader>();
        MockPluginManager = new Mock<IPluginManager>();
        MockServiceProvider = new Mock<IServiceProvider>();
        MockPluginManagerLogger = new Mock<ILogger<PluginManager>>();
        MockPluginLoaderLogger = new Mock<ILogger<PluginLoader>>();

        MockWebHostEnvironment.Setup(we => we.ContentRootPath).Returns(HostPath);

        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton(MockWebHostEnvironment.Object);
        services.AddSingleton<IFileSystem>(MockFileSystem);
        services.AddLogging();

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.GetRequiredService<IPluginManager>();
    }
}