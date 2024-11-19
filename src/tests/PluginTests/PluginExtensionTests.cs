using Backend.Services.Plugins;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace PluginTests;

[TestSubject(typeof(PluginExtensions))]
public class PluginExtensionTests
{
    [Fact]
    public async Task StartPluginSystemAsync_MapsPlugins()
    {
        // Arrange
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions());
        var pluginLoader = new Mock<IPluginLoader>();
        var pluginManager = new Mock<IPluginManager>();
        builder.Services.AddSingleton(pluginLoader.Object);
        builder.Services.AddSingleton(pluginManager.Object);
        var app = builder.Build();
        
        // Act
        await app.StartPluginSystemAsync();
        
        // Assert
        pluginManager.Verify(x => x.MapPlugins(app), Times.Once);
    }
    
    [Fact]
    public async Task StartPluginSystemAsync_ConfiguresServices()
    {
        // Arrange
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions());
        var pluginLoader = new Mock<IPluginLoader>();
        var pluginManager = new Mock<IPluginManager>();
        builder.Services.AddSingleton(pluginLoader.Object);
        builder.Services.AddSingleton(pluginManager.Object);
        var app = builder.Build();
        
        // Act
        await app.StartPluginSystemAsync();
        
        // Assert
        pluginManager.Verify(x => x.ConfigureServices(), Times.Once);
    }
    
    [Fact]
    public async Task StartPluginSystemAsync_LaunchesPluginSystem()
    {
        // Arrange
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions());
        var pluginLoader = new Mock<IPluginLoader>();
        var pluginManager = new Mock<IPluginManager>();
        builder.Services.AddSingleton(pluginLoader.Object);
        builder.Services.AddSingleton(pluginManager.Object);
        var app = builder.Build();
        
        // Act
        await app.StartPluginSystemAsync();
        
        // Assert
        pluginManager.Verify(x => x.StartAsync(), Times.Once);
    }
    
    [Fact]
    public async Task LoadPluginsAsync_LoadsDefaultPlugins()
    {
        // Arrange
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions());
        var pluginLoader = new Mock<IPluginLoader>();
        builder.Services.AddSingleton(pluginLoader.Object);
        var app = builder.Build();
        
        // Act
        await app.LoadPluginsAsync();
        
        // Assert
        pluginLoader.Verify(x => x.LoadDefaultPluginsAsync(), Times.Once);
    }
}