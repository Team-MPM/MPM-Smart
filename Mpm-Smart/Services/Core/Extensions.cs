using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Services.Core;

public static class Extensions
{
    public static WebApplication MapDefaultMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        
        app.UseRouting();
        
        var services = app.Services;
        if (services.GetService(typeof(IAuthenticationService)) is not null)
        {
            app.UseAuthentication();

            if (services.GetService(typeof(IdentityServerOptions)) is not null)
            {
                app.UseIdentityServer();
            }
            
            if (services.GetService(typeof(IAuthorizationService)) is not null)
            {
                app.UseAuthorization();
            }
        }
            
        app.MapFallback(() => "404 Page not found");
        
        return app;
    }
}