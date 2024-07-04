namespace MPM_Betting.Aspire.AppHost;

public class Neo4JResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    private EndpointReference? m_Neo4JReference;
    private EndpointReference Neo4JEndpoint => m_Neo4JReference ??= new EndpointReference(this, "neo4j");
    
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"neo4j://{Neo4JEndpoint.Property(EndpointProperty.Host)}:{Neo4JEndpoint.Property(EndpointProperty.Port)}"
    );
}

public static class Neo4JContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "neo4j";
    public const string Tag = "5.21.0";
}

public static class Neo4JResourceBuilderExtensions
{
    public static IResourceBuilder<Neo4JResource> AddNeo4J(this IDistributedApplicationBuilder builder, string name, 
        int httpPort, int neo4JPort, IResourceBuilder<ParameterResource> password)
    {
        var resource = new Neo4JResource(name);
        return builder.AddResource(resource)
            .WithImage(Neo4JContainerImageTags.Image)
            .WithImageRegistry(Neo4JContainerImageTags.Registry)
            .WithImageTag(Neo4JContainerImageTags.Tag)
            .WithHttpEndpoint(httpPort, 7474, name: "http")
            .WithHttpEndpoint(neo4JPort, 7687, name: "neo4j")
            .WithEnvironment("NEO4J_AUTH", $"neo4j/{password.Resource}")
            .WithVolume("data", "/data");
    }
}