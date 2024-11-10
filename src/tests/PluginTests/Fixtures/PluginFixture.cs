using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Reflection;
using Backend.Services;
using Backend.Services.Plugins;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TestBase.Helpers;

namespace PluginTests.Fixtures;

public class PluginFixture
{
    public Mock<IPluginLoader> MockPluginLoader { get; private set; }
    public Mock<IPluginManager> MockPluginManager { get; private set; }
    public Mock<IWebHostEnvironment> MockWebHostEnvironment { get; private set; }
    
    public ServiceProvider ServiceProvider { get; private set; }
    public MockFileSystem MockFileSystem { get; set; }
    
    public int PluginCount { get; private set; }

    public PluginFixture()
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var pluginSourceDir = Path.Combine(assemblyLocation,
            "../../../../../../../build/plugins");
        var pluginTargetDir = Path.Combine(assemblyLocation, "../plugins");
        var appPath = Path.Combine(assemblyLocation, "../temp/app");
        
        const string testPlugin1Json = $$"""
                                         {
                                           "Name": "Test Plugin 1",
                                           "RegistryName": "test-plugin1",
                                           "Description": "A test plugin",
                                           "Author": "Gabriel Martin",
                                           "Version": "1.0.0"
                                         }
                                         """;
        
        var testPluginAssemblyBin = File.ReadAllBytes(pluginSourceDir + "/test-plugin/TestPlugin.dll");

        MockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>()
        {
            {
                "mpm-smart/bin/plugins/test/plugin.json",
                new MockFileData(testPlugin1Json)
            },
            {
                "mpm-smart/bin/plugins/test/TestPlugin.dll",
                new MockFileData(testPluginAssemblyBin)
            }
        }, Path.Combine(assemblyLocation, "../app/bin"));
        
        PluginCount = 1;

        var services = new ServiceCollection();

        MockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        MockPluginLoader = new Mock<IPluginLoader>();
        MockPluginManager = new Mock<IPluginManager>();

        services.AddSingleton(MockPluginLoader.Object);
        services.AddSingleton(MockPluginManager.Object);
        services.AddSingleton(MockWebHostEnvironment.Object);
        services.AddSingleton<IFileSystem>(MockFileSystem);
        services.AddLogging();

        ServiceProvider = services.BuildServiceProvider();
    }
}