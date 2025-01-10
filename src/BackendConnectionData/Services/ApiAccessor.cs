using System.Net.Http.Json;
using ApiSchema.Enums;
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
    public async Task<ResponseModel<string>> Login(string username, string password)
    {
        var response = await GetResponseModel<string>(async (client) => await client.PostAsJsonAsync("/api/identity/login", new LoginModel() { UserName = username, Password = password }));
        if(response.Success)
            response.Response = response.Response!.Trim('\"');
        return response;
    }

    public async Task<ResponseModel> Logout()
    {
        try
        {
            await LocalStorageService.RemoveItemAsync("authToken");
            return new()
            {
                Success = true,
                StatusCode = 200
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                StatusCode = 500,
                Success = false,
                Message = e.Message
            };
        }
    }

    // ---------------------------- ----------------------------

    private async Task<ResponseModel<T>> GetResponseModel<T>(Func<HttpClient, Task<HttpResponseMessage>> request) where T : class
    {
        try
        {
            var response = await request(Client);
            if (response.IsSuccessStatusCode)
            {
                if (typeof(T) == typeof(object))
                    return new ResponseModel<T>()
                    {
                        Success = true,
                        StatusCode = (int)response.StatusCode,
                        Message = await response.Content.ReadAsStringAsync()
                    };
                var result = await response.Content.ReadFromJsonAsync<T>();
                return new ResponseModel<T>()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = result
                };
            }

            return new ResponseModel<T>()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };

        }
        catch(Exception e)
        {
            return new ResponseModel<T>()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    // ---------------------------- PROFILE ----------------------------

    public async Task<ResponseModel<UserData>> GetUserData()
    {
        return await GetResponseModel<UserData>(async (client) => await Client.GetAsync($"api/profile/info"));
    }

    public async Task<ResponseModel> SetUsername(string newUsername)
    {
        var response = await GetResponseModel<object>(async (client) => await client.PostAsJsonAsync("api/profile/username", new UsernameModel() { Username = newUsername }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetPassword(string currentPassword, string newPassword)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/profile/password", new PasswordModel() { CurrentPassword = currentPassword, NewPassword = newPassword }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetLanguage(int language)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/profile/language", new LanguageModel() { Language = language }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions()
    {
        return await GetResponseModel<PermissionsModel>(async (client) => await Client.GetAsync("api/profile/permissions"));
    }

    // ---------------------------- SETTINGS ----------------------------

    public async Task<ResponseModel<SettingsModel>> GetSettings()
    {
        return await GetResponseModel<SettingsModel>(async (client) => await Client.GetAsync("api/settings/"));
    }

    public async Task<ResponseModel> SetSystemName(string newSystemName)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/settings/systemname", new SystemNameModel() { SystemName = newSystemName }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetSystemTime(string newSystemTime)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/settings/systemtime", new SystemTimeModel() { TimeZoneCode = newSystemTime }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetTimeBetweenUpdates(int newTimeBetweenUpdates)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/settings/timebetweenupdates",
            new TimeBetweenUpdatesModel() { TimeBetweenUpdatesSeconds = newTimeBetweenUpdates }));
        return ResponseModel<object>.Parse(response);
    }

    // ---------------------------- Permissions ----------------------------

    public async Task<ResponseModel<Dictionary<string, List<string>>>> GetAllPermissions()
    {
        return await GetResponseModel<Dictionary<string, List<string>>>(async (client) => await Client.GetAsync("api/permissions/all"));
    }

    public async Task<ResponseModel<PermissionsModel>> GetPermissionsForUser(string user)
    {
        return await GetResponseModel<PermissionsModel>(async (client) => await Client.GetAsync($"api/permissions/user/{user}"));
    }

    public async Task<ResponseModel> SetPermissionsForUser(string user, AddPermissionsModel model)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/permissions/user/{user}", model));
        return ResponseModel<object>.Parse(response);
    }

    // TODO IMPLEMENT ROLE PERMISSIONS -> NOT YET NEEDED

    // ---------------------------- USER MANAGEMENT ----------------------------

    public async Task<ResponseModel<List<UsersModel>>> GetAllUsers()
    {
        return await GetResponseModel<List<UsersModel>>(async (client) => await Client.GetAsync("api/users/all"));
    }

    public async Task<ResponseModel<UsersModel>> GetSpecificUserInfo(string user)
    {
        return await GetResponseModel<UsersModel>(async (client) => await Client.GetAsync($"api/users/{user}"));
    }

    public async Task<ResponseModel> AddNewUser(AddUserModel model)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync("api/users/add", model));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> DeleteUser(string user)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.DeleteAsync($"api/users/{user}"));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetUsernameForUser(string user, string newUsername)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/users/{user}/username", new ChangeUsernameModel() { Username = newUsername }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetPasswordForUser(string user, string newPassword)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/users/{user}/password", new ChangePasswordModel() { NewPassword = newPassword }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetIsActiveForUser(string user, bool isActive)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/users/{user}/isactive", new ChangeIsActiveModel() { IsActive = isActive }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetLanguageForUser(string user, int language)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/users/{user}/language", new ChangeLanguageModel() { Language = (Language) language }));
        return ResponseModel<object>.Parse(response);
    }

    public async Task<ResponseModel> SetIsDarkModeForUser(string user, bool useDarkMode)
    {
        var response = await GetResponseModel<object>(async (client) => await Client.PostAsJsonAsync($"api/users/{user}/isdarkmode", new UseDarkModeModel() { UseDarkMode = useDarkMode }));
        return ResponseModel<object>.Parse(response);
    }



    // ---------------------------- PLUGIN DATA ----------------------------
    
}


