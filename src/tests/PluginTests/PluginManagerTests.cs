using Backend.Services;
using JetBrains.Annotations;
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
            Assert.NotEmpty(plugin.Endpoints);
        });
    }
}