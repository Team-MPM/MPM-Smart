using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;

namespace Frontend.Services;

public record ControllerConnectionDetails(string Address, int Port, bool UseHttps);

public record ControllerCredentials;

public record ControllerPasswordCredentials(string Username, string Password, ILocalStorageService Storage)
    : ControllerCredentials;

public record ControllerTokenCredentials(string Token) : ControllerCredentials;

public record ControllerStoredCredentials(ILocalStorageService Storage) : ControllerCredentials;

public class ControllerConnectionManager(IServiceProvider sp)
{
    public string? RefreshToken { get; set; }
    private HttpClient m_Client = new();
    public string? Token { get; private set; }
    private bool m_Connected = false;
    private ApiAccessor m_Api = null!;
    private Uri? m_Uri;
    public ControllerConnectionDetails? Details { get; private set; } = null;

    public event Action? OnChange;

    public void Init()
    {
        m_Api ??= sp.GetRequiredService<ApiAccessor>();
    }

    public async Task<bool> ConnectToControllerAsync(ControllerConnectionDetails details,
        ControllerCredentials credentials, CustomAuthStateProvider auth)
    {
        m_Client = new HttpClient();
        var protocol = details.UseHttps ? "https" : "http";
        m_Uri = new Uri($"{protocol}://{details.Address}:{details.Port}/");
        m_Client.BaseAddress = m_Uri;
        m_Client.Timeout = TimeSpan.FromMinutes(1);

        HttpResponseMessage res;

        try
        {
            res = await m_Client.GetAsync("/info");
        }
        catch (Exception)
        {
            return false;
        }

        Details = details;

        if (!res.IsSuccessStatusCode)
        {
            m_Connected = false;
            return false;
        }

        var stream = await res.Content.ReadAsStreamAsync();

        var jsonDocument = await JsonDocument.ParseAsync(stream);
        var root = jsonDocument.RootElement;
        var version = root.GetProperty("version").GetString();
        var system = root.GetProperty("system").GetString();
        var id = root.GetProperty("id").GetString();

        if (version is null || system is null || id is null)
        {
            m_Connected = false;
            return false;
        }

        m_Connected = true;
        m_Api.Details = details;

        switch (credentials)
        {
            case ControllerTokenCredentials tokenCredentials:
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenCredentials.Token);
                Token = tokenCredentials.Token;
                auth.NotifyUserAuthentication(tokenCredentials.Token);
                break;
            case ControllerPasswordCredentials passwordCredentials:
                var tokenResponse = await m_Api.Login(passwordCredentials.Username, passwordCredentials.Password);
                if (!tokenResponse.Success || tokenResponse.Response is null)
                    return false;

                await passwordCredentials.Storage
                    .SetItemAsStringAsync($"authToken-{details.Address}:{details.Port}",
                        tokenResponse.Response.Token);
                await passwordCredentials.Storage
                    .SetItemAsStringAsync($"refreshToken-{details.Address}:{details.Port}",
                        tokenResponse.Response.RefreshToken!);
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.Response.Token);
                Token = tokenResponse.Response.Token;
                auth.NotifyUserAuthentication(tokenResponse.Response.Token);
                break;
            case ControllerStoredCredentials storedCredentials:
                var token = await storedCredentials.Storage
                    .GetItemAsStringAsync($"authToken-{details.Address}:{details.Port}");
                if (token is null)
                    return false;
                if (new JwtSecurityToken(token).ValidTo < DateTime.UtcNow)
                {
                    var result = await m_Api.TryRefreshToken();
                    if (!result.Success)
                        return false;
                    await storedCredentials.Storage
                        .SetItemAsStringAsync($"authToken-{details.Address}:{details.Port}",
                            result.Response!);
                }
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                Token = token;
                auth.NotifyUserAuthentication(token!);
                break;
        }

        OnChange?.Invoke();
        return true;
    }

    public void DisconnectFromController(CustomAuthStateProvider auth)
    {
        Token = null;
        m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");
        m_Connected = false;
        OnChange?.Invoke();
        auth.NotifyUserLogout();
    }

    public HttpClient? GetCurrentClient() => m_Connected ? m_Client : null;

    public HubConnectionBuilder GetSignalRClient(string path)
    {
        if (m_Uri is null) throw new InvalidOperationException("Not connected to a controller");
        var builder = new HubConnectionBuilder();
        builder.WithUrl(new Uri(m_Uri, path), options =>
        {
            options.AccessTokenProvider = () => Task.FromResult(Token);
        });

        return builder;
    }
}