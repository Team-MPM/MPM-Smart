using System.Net.Http.Json;
using System.Text.Json;
using ApiSchema.Identity;
using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using BackendConnectionData.Model;
using Blazored.LocalStorage;
using PermissionsModel = BackendConnectionData.Model.PermissionsModel;

namespace BackendConnectionData.Services;

public class ApiAccessor
{
    private HttpClient Client { get; }
    private ILocalStorageService LocalStorageService { get; set; }
    private CustomAuthStateProvider AuthStateProvider { get; set; }
    public ApiAccessor(HttpClient httpClient, ILocalStorageService localStorageService, CustomAuthStateProvider authStateProvider)
    {
        Client = httpClient;
        LocalStorageService = localStorageService;
        AuthStateProvider = authStateProvider;
    }

    // ---------------------------- IDENTITY ----------------------------

    public async Task<ResponseModel> Login(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/identity/login", new LoginModel() { UserName = username, Password = password });
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<string>();
            result = result!.Trim('\"');
            await LocalStorageService.SetItemAsync("authToken", result);
            AuthStateProvider.NotifyUserAuthentication(result);
            return new ResponseModel()
            {
                Success = true,
                StatusCode = (int)response.StatusCode,
            };
        }

        return new ResponseModel()
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Message = await response.Content.ReadAsStringAsync()
        };
    }

    // ---------------------------- PROFILE ----------------------------

    public async Task<ResponseModel<UserData>> GetUserData()
    {
        try
        {
            var response = await Client.GetAsync($"api/profile/info");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserData>(content);
                return new ResponseModel<UserData>()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = userData
                };
            }

            return new ResponseModel<UserData>()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel<UserData>()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetUsername(string newUsername)
    {
        var response =
            await Client.PostAsJsonAsync("api/profile/username", new UsernameModel() { Username = newUsername });
        if (response.IsSuccessStatusCode)
            return new ResponseModel()
            {
                Success = true,
                StatusCode = (int) response.StatusCode
            };
        return new ResponseModel()
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Message = await response.Content.ReadAsStringAsync()
        };
    }

    public async Task<ResponseModel> SetPassword(string currentPassword, string newPassword)
    {
        var response =
            await Client.PostAsJsonAsync("api/profile/password", new PasswordModel() { CurrentPassword = currentPassword, NewPassword = newPassword });
        if (response.IsSuccessStatusCode)
            return new ResponseModel()
            {
                Success = true,
                StatusCode = (int) response.StatusCode
            };
        return new ResponseModel()
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Message = await response.Content.ReadAsStringAsync()
        };
    }

    public async Task<ResponseModel> SetLanguage(int language)
    {
        var response =
            await Client.PostAsJsonAsync("api/profile/language", new LanguageModel() { Language = language });
        if (response.IsSuccessStatusCode)
            return new ResponseModel()
            {
                Success = true,
                StatusCode = (int) response.StatusCode
            };
        return new ResponseModel()
        {
            Success = false,
            StatusCode = (int)response.StatusCode,
            Message = await response.Content.ReadAsStringAsync()
        };
    }

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions()
    {
        var response = await Client.GetAsync("api/profile/permissions");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var permissions = JsonSerializer.Deserialize<PermissionsModel>(content);
            return new ResponseModel<PermissionsModel>()
            {
                Success = true,
                StatusCode = (int)response.StatusCode,
                Response = permissions
            };
        }

        return new ResponseModel<PermissionsModel>()
        {
            Success = false,
            StatusCode = (int) response.StatusCode,
            Message = await response.Content.ReadAsStringAsync()
        };
    }

    // ---------------------------- SETTINGS ----------------------------
}


