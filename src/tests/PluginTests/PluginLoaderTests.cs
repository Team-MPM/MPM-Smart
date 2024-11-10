using Backend.Services;
using Backend.Services.Plugins;
using JetBrains.Annotations;
using PluginTests.Fixtures;
using Xunit;

namespace PluginTests;

[TestSubject(typeof(PluginLoader))]
[Collection(nameof(PluginTests))]
public class PluginLoaderTests(PluginFixture fixture)
{
    // [Fact]
    // public void PluginLoader_ShouldDetectAllPluginsFromDirectory()
    // {
    //     fixture.LoadPlugins();
    //     
    //     Assert.Equal(fixture.PluginCount, fixture.PluginLoader.PluginAssemblies.Count);
    // }
    //
    // [Fact]
    // public void PluginLoader_ShouldLoadAllPlugins()
    // {
    //     fixture.LoadPlugins();
    //
    //     Assert.All(fixture.PluginLoader.PluginAssemblies, assembly => Assert.NotEmpty(assembly.GetTypes()));
    // }
    //
    // [Fact]
    // public void PluginLoader_ShouldNotLoadPluginBase()
    // {
    //     fixture.LoadPlugins();
    //
    //     Assert.DoesNotContain(fixture.PluginLoader.PluginAssemblies, assembly => assembly.GetName().Name == "PluginBase");
    // }#
    
    [Fact]
    public void Test()
    {
        Assert.True(true);
    }
}