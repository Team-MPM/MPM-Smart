using System.Net.Http.Json;
using System.Xml.Serialization;
using ApiSchema.Identity;
using Blazored.LocalStorage;

namespace Frontend.Services;

public class TokenHandler
{
    private ILocalStorageService m_Storage;
    private ControllerConnectionManager m_Manager;
    public TokenHandler(IServiceProvider sp)
    {
        m_Storage = sp.GetRequiredService<ILocalStorageService>();
        m_Manager = sp.GetRequiredService<ControllerConnectionManager>();
    }

    public async Task<ResponseModel<string>> RefreshTokenAsync(HttpClient client, string host, string port, int duration, LoginDurationEntity durationEntity)
    {
        var refreshToken = await m_Storage.GetItemAsStringAsync($"refreshToken-{host}:{port}");
        if (string.IsNullOrEmpty(refreshToken))
            return new ResponseModel<string>().NoClientError();

        var response = await client.PostAsJsonAsync("/api/identity/refresh", new RefreshTokenModel
        {
            RefreshToken = refreshToken,
            Duration = duration,
            DurationType = durationEntity
        });

        if (!response.IsSuccessStatusCode)
            return await new ResponseModel<string>().ServerError(response);

        var newToken = await response.Content.ReadAsStringAsync();
        newToken = newToken.Trim('"');
        await m_Storage.SetItemAsStringAsync($"authToken-{host}:{port}", newToken);

        return await new ResponseModel<string>().SuccessResultAsync(response);
    }

    public async Task LogoutAsync()
    {
        string host = m_Manager.Details.Address;
        string port = m_Manager.Details.Port.ToString();
        await m_Storage.RemoveItemAsync($"authToken-{host}:{port}");
        await m_Storage.RemoveItemAsync($"refreshToken-{host}:{port}");
    }
}