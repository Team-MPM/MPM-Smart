using Backend.Services;
using Backend.Services.Plugins;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Routing;
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
    [Fact]
    public void PluginManager_ShouldRegisterAllPlugins()
    {
        fixture.LoadPlugins();
        Assert.Equal(fixture.PluginCount, fixture.PluginManager.Plugins.Count);
    }
    
    [Fact]
    public void PluginManager_ShouldRegisterAllEndpoints()
    {
        fixture.LoadPlugins();
        
        Assert.All(fixture.PluginManager.Plugins, plugin =>
        {
//TODO
        });
    }

    [Fact]
    public void PluginManager_RegisterPluginShouldInitializePlugin()
    {
        // var logger = new Mock<ILogger<PluginManager>>().Object;
        // var pluginManager = new PluginManager(logger);
        //
        // var plugin = new Mock<IPlugin>();
        // plugin.Setup(p => p.Name).Returns("TestPlugin");
        //
        // pluginManager.RegisterPlugin(plugin.Object);
        //
        // plugin.Verify(p => p.Initialize(), Times.Once);
    }
    
    // [Fact]
    // public void PluginManager_DisposeShouldDisposeAllPlugins()
    // {
    //     var logger = new Mock<ILogger<PluginManager>>().Object;
    //     var pluginManager = new PluginManager(logger);
    //     
    //     var plugin = new Mock<IPlugin>();
    //     
    //     pluginManager.RegisterPlugin(plugin.Object);
    //     pluginManager.Dispose();
    //     
    //     plugin.Verify(p => p.Dispose(), Times.Once);
    // }
    //
    // [Fact]
    // public void PluginManager_RegisterPluginShouldAddPluginToPluginsList()
    // {
    //     var logger = new Mock<ILogger<PluginManager>>().Object;
    //     var pluginManager = new PluginManager(logger);
    //     
    //     var plugin = new Mock<IPlugin>();
    //     
    //     pluginManager.RegisterPlugin(plugin.Object);
    //     
    //     Assert.Contains(plugin.Object, pluginManager.Plugins);
    // }
}