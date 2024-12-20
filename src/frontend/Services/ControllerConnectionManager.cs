using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Blazored.LocalStorage;

namespace Frontend.Services;

public record ControllerConnectionDetails(IPAddress Address, int Port);

public class ControllerConnectionManager(ILocalStorageService storage)
{
    private readonly HttpClient m_Client = new();
    private bool m_Connected = false;

    public async Task ConnectToControllerAsync(ControllerConnectionDetails details)
    {
        m_Client.BaseAddress = new Uri($"https://{details.Address}:{details.Port}/");
        m_Client.Timeout = TimeSpan.FromSeconds(5);
        var res = await m_Client.GetAsync("/info");

        if (!res.IsSuccessStatusCode)
        {
            m_Connected = false;
            return;
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
            return;
        }

        var token = await storage.GetItemAsStringAsync($"authToken-{id}");

        if (token is not null)
            m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        m_Connected = true;
    }

    public void DisconnectFromController()
    {
        m_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "");
        m_Connected = false;
    }

    public HttpClient? GetCurrentClient() => m_Connected ? m_Client : null;
}