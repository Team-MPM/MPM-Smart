using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace BackendConnectionData.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService LocalStorageService;
    private readonly HttpClient _client;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    public CustomAuthStateProvider(ILocalStorageService localStorageService, HttpClient client, ILogger<CustomAuthStateProvider> logger)
    {
        LocalStorageService = localStorageService;
        _client = client;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await LocalStorageService.GetItemAsStringAsync("authToken");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("No token found, returning anonymous user.");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        token = token.Trim('\"');

        try
        {
            var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            Console.WriteLine("Returning authenticated user.");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing token: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }


    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "Bearer");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task NotifyUserLogout()
    {
        await LocalStorageService.RemoveItemAsync("authToken");
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
           _logger.LogInformation($"Type: {claim.Type}, Value: {claim.Value}");
        }

        return token.Claims;
    }
}