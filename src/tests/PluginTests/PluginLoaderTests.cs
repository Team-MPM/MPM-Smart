using Backend.Services;
using Backend.Services.Plugins;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PluginTests.Fixtures;
using Xunit;

namespace PluginTests;

[TestSubject(typeof(PluginLoader))]
[Collection(nameof(PluginTests))]
public class PluginLoaderTests(PluginFixture fixture)
{
    [Fact]
    public void Constructor_BuildsInstance_WhenLoadedAsService()
    {
        // Arrange
        fixture.Setup();

        // Act
        var pluginLoader = fixture.ServiceProvider.GetRequiredService<IPluginLoader>();

        // Assert
        pluginLoader.Should().BeOfType(typeof(PluginLoader));
    }

    [Fact]
    public async Task LoadDefaultPluginsAsync_LoadsDefaultPlugins()
    {
        // Arrange
        fixture.Setup();

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        await pluginLoader.LoadDefaultPluginsAsync();

        // Assert
        pluginLoader.PluginAssemblies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoadDefaultPluginsAsync_ReturnsImmediately_WhenNoPluginsDirectoryExists()
    {
        // Arrange
        fixture.Setup();
        var pluginsDirectory = Path.Combine(fixture.HostPath, "..", "..", "plugins");
        fixture.MockFileSystem.Directory.Delete(pluginsDirectory, true);

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        await pluginLoader.LoadDefaultPluginsAsync();

        // Assert
        pluginLoader.PluginAssemblies.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadDefaultPluginsAsync_LogsWarning_WhenNoPluginsDirectoryExists()
    {
        // Arrange
        fixture.Setup();
        var pluginsDirectory = Path.Combine(fixture.HostPath, "..", "..", "plugins");
        fixture.MockFileSystem.Directory.Delete(pluginsDirectory, true);

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        await pluginLoader.LoadDefaultPluginsAsync();

        // Assert
        fixture.MockPluginLoaderLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No plugins directory found at")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public async Task LoadDefaultPluginsAsync_SetsPluginsLoaded_WhenNoPluginsDirectoryExists()
    {
        // Arrange
        fixture.Setup();
        var pluginsDirectory = Path.Combine(fixture.HostPath, "..", "..", "plugins");
        fixture.MockFileSystem.Directory.Delete(pluginsDirectory, true);

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        await pluginLoader.LoadDefaultPluginsAsync();

        // Assert
        pluginLoader.WaitForPluginsToLoadAsync().IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task LoadDefaultPluginsAsync_SetsPluginsLoaded_WhenPluginsDirectoryExists()
    {
        // Arrange
        fixture.Setup();

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        await pluginLoader.LoadDefaultPluginsAsync();

        // Assert
        pluginLoader.WaitForPluginsToLoadAsync().IsCompleted.Should().BeTrue();
    }
    
    [Fact]
    public void Dispose_DisposesPluginAssemblies()
    {
        // Arrange
        fixture.Setup();

        var pluginLoader = new PluginLoader(
            fixture.MockPluginManager.Object,
            fixture.MockWebHostEnvironment.Object,
            fixture.MockPluginLoaderLogger.Object,
            fixture.MockFileSystem);

        // Act
        pluginLoader.Dispose();

        // Assert
        foreach (var assembly in pluginLoader.PluginAssemblies)
        {
            assembly.IsDynamic.Should().BeTrue();
            assembly.IsCollectible.Should().BeTrue();
        }
    }
}