using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public class CustomAuthStateProvider(
    ControllerConnectionManager controllerConnectionManager,
    ILogger<CustomAuthStateProvider> logger)
    : AuthenticationStateProvider
{
    public ClaimsPrincipal? User { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = controllerConnectionManager.Token;

        if (string.IsNullOrEmpty(token))
        {
            logger.LogInformation("No token found, returning anonymous user.");
            User = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(User));
        }

        token = token.Trim('\"');

        try
        {
            var jwtToken = new JwtSecurityToken(token);
            if(!CheckTokenValidity(jwtToken))
            {
                logger.LogInformation("Token expired, returning anonymous user.");
                User = new ClaimsPrincipal(new ClaimsIdentity());
                return Task.FromResult(new AuthenticationState(User));
            }
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            User = new ClaimsPrincipal(identity);
            logger.LogInformation("Returning authenticated user.");
            return Task.FromResult(new AuthenticationState(User));
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error parsing token: {Message}", ex.Message);
            User = new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(User));
        }
    }

    private bool CheckTokenValidity(JwtSecurityToken token) => token.ValidTo > DateTime.UtcNow;

    public void NotifyUserAuthentication(string token)
    {
        Console.WriteLine(token);
        token = token.Replace("\"", "");
        Console.WriteLine(token);
        if (!CheckTokenValidity(new JwtSecurityToken(token.Replace("\"", ""))))
        {
            logger.LogInformation("Token expired, returning anonymous user.");
            User = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(User)));
            return;
        }
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "Bearer");
        User = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(User)));
    }

    public void NotifyUserLogout()
    {
        User = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(User)));
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