using PluginTests.Fixtures;
using Xunit;

namespace PluginTests;

[CollectionDefinition(nameof(PluginTests))]
public class PluginTestCollection : ICollectionFixture<PluginFixture>
{
    
}