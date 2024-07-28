using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Services.Identity;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("user", "MPM-Smart User"),
            new ApiScope("admin", "MPM-Smart Admin"),
        };
    
    public static IEnumerable<ApiResource> ApiResources =>
        new List<ApiResource>
        {
            new ApiResource("user", "MPM-Smart User")
            {
                Scopes = { "user" }
            },
            new ApiResource("admin", "MPM-Smart Admin")
            {
                Scopes = { "admin" }
            }
        };
    
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "web-client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "user", "admin" }
            },
            new Client
            {
                ClientId = "mobile-app-client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "user" }
            }
        };
}