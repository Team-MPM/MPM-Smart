namespace MPM_Betting.Aspire.AppHost;

// based on https://github.com/dotnet/docs-aspire/pull/755/files#conversations-menu

public class MailDevResource(string name) : ContainerResource(name), IResourceWithConnectionString
{
    private EndpointReference? m_SmtpReference;
    private EndpointReference SmtpEndpoint => m_SmtpReference ??= new EndpointReference(this, "smtp");
    
    public ReferenceExpression ConnectionStringExpression => ReferenceExpression.Create(
        $"smtp://{SmtpEndpoint.Property(EndpointProperty.Host)}:{SmtpEndpoint.Property(EndpointProperty.Port)}"
    );
}

public static class MailDevContainerImageTags
{
    public const string Registry = "docker.io";
    public const string Image = "maildev/maildev";
    public const string Tag = "2.0.2";
}

public static class MailDevResourceBuilderExtensions
{
    public static IResourceBuilder<MailDevResource> AddMailDev(this IDistributedApplicationBuilder builder, string name, int? httpPort = null, int? smtpPort = null)
    {
        var resource = new MailDevResource(name);
        return builder.AddResource(resource)
            .WithImage(MailDevContainerImageTags.Image)
            .WithImageRegistry(MailDevContainerImageTags.Registry)
            .WithImageTag(MailDevContainerImageTags.Tag)
            .WithHttpEndpoint(httpPort, 1080, name: "http")
            .WithEndpoint(smtpPort, 1025, name: "smtp");
    }
}