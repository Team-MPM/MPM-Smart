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

public record ControllerStoredCredentials(ILocalStorageService Storage) : ControllerCredentials;

public class ControllerConnectionManager(IServiceProvider sp)
{
    private HttpClient m_Client = new();
    private bool m_Connected = false;
    private ApiAccessor m_Api = null!;

    public void Init()
    {
        m_Api ??= sp.GetRequiredService<ApiAccessor>();
    }

    public async Task<bool> ConnectToControllerAsync(ControllerConnectionDetails details,
        ControllerCredentials credentials, CustomAuthStateProvider auth)
    {
        Init();
        m_Client = new HttpClient();
        m_Client.BaseAddress = new Uri($"https://{details.Address}:{details.Port}/");
        m_Client.Timeout = TimeSpan.FromSeconds(5);

        HttpResponseMessage res;

        try
        {
            res = await m_Client.GetAsync("/info");
        }
        catch (Exception)
        {
            return false;
        }

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

        switch (credentials)
        {
            case ControllerTokenCredentials tokenCredentials:
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenCredentials.Token);
                auth.NotifyUserAuthentication(tokenCredentials.Token);
                break;
            case ControllerPasswordCredentials passwordCredentials:
                var tokenResponse = await m_Api.Login(passwordCredentials.Username, passwordCredentials.Password);
                Console.WriteLine(tokenResponse.Success);
                if (!tokenResponse.Success || tokenResponse.Response is null)
                    return false;

                await passwordCredentials.Storage
                    .SetItemAsStringAsync($"authToken-{details.Address.ToString()}:{details.Port}",
                        tokenResponse.Response);
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", tokenResponse.Response);
                auth.NotifyUserAuthentication(tokenResponse.Response);
                break;
            case ControllerStoredCredentials storedCredentials:
                var token = await storedCredentials.Storage
                    .GetItemAsStringAsync($"authToken-{details.Address.ToString()}:{details.Port}");
                if (token is null)
                    return false;
                m_Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                auth.NotifyUserAuthentication(token);
                break;
        }

        return true;
    }

    public void DisconnectFromController(CustomAuthStateProvider auth)
    {
        Init();
        m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");
        m_Connected = false;
        auth.NotifyUserLogout();
    }

    public HttpClient? GetCurrentClient() => m_Connected ? m_Client : null;
}