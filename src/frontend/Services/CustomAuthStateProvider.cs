using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public class CustomAuthStateProvider(
    ControllerConnectionManager controllerConnectionManager,
    ILogger<CustomAuthStateProvider> logger)
    : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = controllerConnectionManager.GetCurrentClient()?
            .DefaultRequestHeaders.Authorization?.Parameter;

        if (string.IsNullOrEmpty(token))
        {
            logger.LogInformation("No token found, returning anonymous user.");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        token = token.Trim('\"');

        try
        {
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            logger.LogInformation("Returning authenticated user.");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error parsing token: {Message}", ex.Message);
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }


    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "Bearer");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);

        Console.WriteLine("Claims in JWT:");
        foreach (var claim in token.Claims)
        {
            logger.LogInformation("Type: {Type}, Value: {Value}", claim.Type, claim.Value);
        }

        return token.Claims;
    }
}