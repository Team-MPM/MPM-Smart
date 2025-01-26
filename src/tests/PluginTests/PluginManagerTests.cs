using Backend.Services.Plugins;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using PluginBase;
using PluginTests.Fixtures;
using Xunit;

namespace PluginTests;

[TestSubject(typeof(PluginManager))]
[Collection(nameof(PluginTests))]
public class PluginManagerTests(PluginFixture fixture)
{
    const string TestPluginPath = "test";

    [Fact]
    public void Constructor_BuildsInstance_WhenLoadedAsService()
    {
        // Arrange
        fixture.Setup();

        // Act
        var pluginLoader = fixture.ServiceProvider.GetRequiredService<IPluginManager>();

        // Assert
        pluginLoader.Should().BeOfType(typeof(PluginManager));
    }

    [Fact]
    public void RegisterPlugin_InitializesPlugin()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        mockPlugin.Verify(p => p.OnInitialize(fixture.MockServiceProvider.Object,
            TestPluginPath, fixture.HostPath), Times.Once);
    }

    [Fact]
    public void RegisterPlugin_LogsPluginId()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        fixture.MockPluginManagerLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(mockPlugin.Object.Guid.ToString())),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void RegisterPlugin_LogsError_WhenInitializationFails()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        mockPlugin.Setup(p => p.OnInitialize(It.IsAny<IServiceProvider>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Test exception"));

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        fixture.MockPluginManagerLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void RegisterPlugin_ReturnTrue_WhenInitializationPasses()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        var result = pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RegisterPlugin_ReturnFalse_WhenInitializationFails()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        mockPlugin.Setup(p => p.OnInitialize(It.IsAny<IServiceProvider>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Test exception"));

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        var result = pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void RegisterPlugin_AddPluginToPluginList()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        // Assert
        pluginManager.Plugins.Should().Contain(mockPlugin.Object);
    }

    [Fact]
    public void MapPlugins_CallsOnEndpointBuilding()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin1 = new Mock<IPlugin>();
        var mockPlugin2 = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin1.Object, TestPluginPath);
        pluginManager.RegisterPlugin(mockPlugin2.Object, TestPluginPath);

        var mockRouteBuilder = new Mock<IEndpointRouteBuilder>();

        // Act
        pluginManager.MapPlugins(mockRouteBuilder.Object);

        // Assert
        mockPlugin1.Verify(p => p.OnEndpointBuilding(mockRouteBuilder.Object), Times.Once);
        mockPlugin2.Verify(p => p.OnEndpointBuilding(mockRouteBuilder.Object), Times.Once);
    }

    [Fact]
    public void Dispose_DisposesAllPlugins()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin1 = new Mock<IPlugin>();
        var mockPlugin2 = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.MockServiceProvider.Object,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin1.Object, TestPluginPath);
        pluginManager.RegisterPlugin(mockPlugin2.Object, TestPluginPath);

        // Act
        pluginManager.Dispose();

        // Assert
        mockPlugin1.Verify(p => p.Dispose(), Times.Once);
        mockPlugin2.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_StopsHostedServices()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var mockHostedService = new Mock<BackgroundService>();

        mockPlugin.Setup(p => p.OnConfiguring(It.IsAny<IServiceCollection>()))
            .Callback<IServiceCollection>(services =>
            {
                services.AddSingleton(mockHostedService.Object);
                services.AddHostedService(sp => sp.GetRequiredService<BackgroundService>());
            });

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        pluginManager.ConfigureServices();

        // Act
        pluginManager.Dispose();

        // Assert
        mockHostedService.Verify(hs => hs.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ConfigureServices_CallsOnConfiguring()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin1 = new Mock<IPlugin>();
        var mockPlugin2 = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin1.Object, TestPluginPath);
        pluginManager.RegisterPlugin(mockPlugin2.Object, TestPluginPath);

        // Act
        pluginManager.ConfigureServices();

        // Assert
        mockPlugin1.Verify(p => p.OnConfiguring(It.IsAny<IServiceCollection>()), Times.Once);
        mockPlugin2.Verify(p => p.OnConfiguring(It.IsAny<IServiceCollection>()), Times.Once);
    }

    [Fact]
    public void ConfigureServices_CreatedServiceProvider()
    {
        // Arrange
        fixture.Setup();

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        pluginManager.ConfigureServices();

        // Assert
        pluginManager.PluginServices.Should().NotBeNull();
    }

    [Fact]
    public void StartAsync_ThrowsException_WhenPluginServicesNotConfigured()
    {
        // Arrange
        fixture.Setup();

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        // Act
        var action = () => pluginManager.StartAsync().GetAwaiter().GetResult();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task StartAsync_CallsOnSystemStart()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin1 = new Mock<IPlugin>();
        var mockPlugin2 = new Mock<IPlugin>();

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin1.Object, TestPluginPath);
        pluginManager.RegisterPlugin(mockPlugin2.Object, TestPluginPath);

        pluginManager.ConfigureServices();

        // Act
        await pluginManager.StartAsync();

        // Assert
        mockPlugin1.Verify(p => p.OnSystemStart(pluginManager.PluginServices!), Times.Once);
        mockPlugin2.Verify(p => p.OnSystemStart(pluginManager.PluginServices!), Times.Once);
    }

    [Fact]
    public async Task StartAsync_CallsStartAsyncOnHostedServices()
    {
        // Arrange
        fixture.Setup();

        var mockPlugin = new Mock<IPlugin>();

        var mockHostedService = new Mock<BackgroundService>();

        mockPlugin.Setup(p => p.OnConfiguring(It.IsAny<IServiceCollection>()))
            .Callback<IServiceCollection>(services =>
            {
                services.AddSingleton(mockHostedService.Object);
                services.AddHostedService(sp => sp.GetRequiredService<BackgroundService>());
            });

        var pluginManager = new PluginManager(
            fixture.ServiceProvider,
            fixture.MockPluginManagerLogger.Object,
            fixture.MockWebHostEnvironment.Object);

        pluginManager.RegisterPlugin(mockPlugin.Object, TestPluginPath);

        pluginManager.ConfigureServices();

        // Act
        await pluginManager.StartAsync();

        // Assert
        mockHostedService.Verify(hs => hs.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}