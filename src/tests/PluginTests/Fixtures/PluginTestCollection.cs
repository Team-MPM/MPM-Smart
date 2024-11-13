using Xunit;

namespace PluginTests.Fixtures;

[CollectionDefinition(nameof(PluginTests))]
public class PluginTestCollection : ICollectionFixture<PluginFixture>
{
    
}