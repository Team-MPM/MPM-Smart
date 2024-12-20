using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;

namespace Frontend.Services;

public record ControllerConnectionDetails(IPAddress Address, int Port);

public record ControllerCredentials;

public record ControllerPasswordCredentials(string Username, string Password, ILocalStorageService Storage)
    : ControllerCredentials;

public record ControllerTokenCredentials(string Token) : ControllerCredentials;

public class ControllerConnectionManager(IServiceProvider sp)
{
    private readonly HttpClient m_Client = new();
    private bool m_Connected = false;
    private ApiAccessor m_Api = null!;
    private CustomAuthStateProvider m_Auth = null!;

    public void Init()
    {
        m_Api ??= sp.GetRequiredService<ApiAccessor>();
        m_Auth ??= sp.GetRequiredService<CustomAuthStateProvider>();
    }

    public async Task<bool> ConnectToControllerAsync(ControllerConnectionDetails details,
        ControllerCredentials credentials)
    {
        Init();
        m_Client.BaseAddress = new Uri($"https://{details.Address}:{details.Port}/");
        m_Client.Timeout = TimeSpan.FromSeconds(5);
        var res = await m_Client.GetAsync("/info");

        if (!res.IsSuccessStatusCode)
        {
            m_Connected = false;
            return false;
        }

        var stream = await res.Content.ReadAsStreamAsync();

        var jsonDocument = await JsonDocument.ParseAsync(stream);
        var root = jsonDocument.RootElement;
        var version = root.GetProperty("Version").GetString();
        var system = root.GetProperty("System").GetString();
        var id = root.GetProperty("Id").GetString();

        if (version is null || system is null || id is null)
        {
            m_Connected = false;
            return false;
        }

        m_Connected = true;

        switch (credentials)
        {
            case ControllerTokenCredentials tokenCredentials:
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenCredentials.Token);
                m_Auth.NotifyUserAuthentication(tokenCredentials.Token);
                break;
            case ControllerPasswordCredentials passwordCredentials:
                var tokenResponse = await m_Api.Login(passwordCredentials.Username, passwordCredentials.Password);

                if (!tokenResponse.Success || tokenResponse.Response is null)
                    return false;

                await passwordCredentials.Storage
                    .SetItemAsStringAsync($"authToken-{id}", tokenResponse.Response);
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.Response);
                m_Auth.NotifyUserAuthentication(tokenResponse.Response);
                break;
        }

        return true;
    }

    public void DisconnectFromController()
    {
        Init();
        m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");
        m_Connected = false;
        m_Auth.NotifyUserLogout();
    }

    public HttpClient? GetCurrentClient() => m_Connected ? m_Client : null;
}