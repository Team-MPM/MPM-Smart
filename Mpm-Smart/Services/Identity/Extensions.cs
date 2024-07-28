using DataModel.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Services.Identity;

public static class Extensions
{
    public static IHostApplicationBuilder AddAuth(this IHostApplicationBuilder builder, bool useCookies = false)
    {
        if (useCookies)
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("cookie")
                .AddOpenIdConnect("oidc", options =>
                {
                    var httpEndpoint = builder.Configuration["services:IdentityServer:http:0"];
                    var httpsEndpoint = builder.Configuration["services:IdentityServer:https:0"];
                    options.Authority = httpEndpoint ?? httpsEndpoint ?? throw new InvalidOperationException("IS endpoint not found");
                    
                    options.ClientId = "web-client";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    
                    options.SaveTokens = true;
                    
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.GetClaimsFromUserInfoEndpoint = true;
                    
                    options.MapInboundClaims = false;
                    options.DisableTelemetry = true;
                });
        }
        else
        {
            var authBuilder = builder.Services.AddAuthentication();
        }

        builder.Services.AddAuthorization(o =>
        {
            o.AddPolicy("admin", p =>
            {
                p.RequireRole("admin");
            });
        });

        return builder;
    }

    public static IHostApplicationBuilder AddIdentity(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        return builder;
    }

    public static IHostApplicationBuilder AddIdentityServer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddIdentityServer()
            .AddDeveloperSigningCredential() // TODO
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddInMemoryApiResources(Config.ApiResources)
            .AddAspNetIdentity<ApplicationUser>();

        return builder;
    }
}