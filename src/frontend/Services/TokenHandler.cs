using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Xml.Serialization;
using ApiSchema.Identity;
using Blazored.LocalStorage;
using Frontend.Pages.General;
using Microsoft.AspNetCore.Components;

namespace Frontend.Services;

public class TokenHandler
{
    private ILocalStorageService m_Storage;
    private ControllerConnectionManager m_Manager;
    private NavigationManager Nav { get; set; }
    public TokenHandler(IServiceProvider sp)
    {
        m_Storage = sp.GetRequiredService<ILocalStorageService>();
        m_Manager = sp.GetRequiredService<ControllerConnectionManager>();
        Nav = sp.GetRequiredService<NavigationManager>();
    }

    public async Task<ResponseModel<string>> RefreshTokenAsync(HttpClient client, string host, string port, TimeSpan duration)
    {
        var refreshToken = await m_Storage.GetItemAsStringAsync($"refreshToken-{host}:{port}");
        if (string.IsNullOrEmpty(refreshToken))
            return new ResponseModel<string>().NoClientError();

        var response = await client.PostAsJsonAsync("/api/identity/refresh", new RefreshTokenModel
        {
            RefreshToken = refreshToken,
            Duration = duration
        });

        if (!response.IsSuccessStatusCode)
            return await new ResponseModel<string>().ServerError(response);

        var newToken = await response.Content.ReadAsStringAsync();
        newToken = newToken.Trim('"');
        await m_Storage.SetItemAsStringAsync($"authToken-{host}:{port}", newToken);

        return await new ResponseModel<string>().SuccessResultAsync(response);
    }

    public async Task CheckToken()
    {
        var client = m_Manager.GetCurrentClient()!;
        var relativeUri = new Uri(Nav.Uri).PathAndQuery;
        Console.WriteLine(relativeUri);
        if (relativeUri == "/")
            return;

        try
        {
            if(!(await client.GetAsync("api/settings/tryconnect")).IsSuccessStatusCode)
            {
                Nav.NavigateTo("/");
                return;
            }
        } catch(NullReferenceException)
        {
            Nav.NavigateTo("/");
            return;
        } catch(HttpRequestException)
        {
            Nav.NavigateTo("/");
            return;
        }

        var controller = await m_Storage.GetItemAsync<HomePage.ControllerInfo>("current-controller");
        if (controller is null)
        {
            Nav.NavigateTo("/");
            return;
        }
        var token = await m_Storage.GetItemAsStringAsync($"authToken-{controller.Address}:{controller.Port}");
        var refreshToken = await m_Storage.GetItemAsStringAsync($"refreshToken-{controller.Address}:{controller.Port}");
        if (token is not null)
        {
            if (new JwtSecurityToken(token).ValidTo > DateTime.UtcNow)
            {
                return;
            }
        }

        if (refreshToken is null)
        {
            Nav.NavigateTo("/");
            return;
        }

        var result = await RefreshTokenAsync(client, controller.Address, controller.Port.ToString(), TimeSpan.FromDays(2));
        if (!result.Success)
            Nav.NavigateTo("/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Response!);
        var checkResult = await client.GetAsync("api/identity/checktoken");
        if (!checkResult.IsSuccessStatusCode)
            Nav.NavigateTo("/");
        Nav.NavigateTo(Nav.Uri);
        Nav.Refresh();
    }

    public async Task LogoutAsync()
    {
        try
        {
            string host = m_Manager.Details!.Address;
            string port = m_Manager.Details.Port.ToString();
            await m_Storage.RemoveItemAsync($"authToken-{host}:{port}");
            await m_Storage.RemoveItemAsync($"refreshToken-{host}:{port}");
        } catch(NullReferenceException)
        {
        }
    }
}