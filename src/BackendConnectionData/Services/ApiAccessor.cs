using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<ResponseModel> Login(string username, string password)
    {
        try
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
        catch(Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
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

    // ---------------------------- PROFILE ----------------------------

    public async Task<ResponseModel<UserData>> GetUserData()
    {
        try
        {
            var response = await Client.GetAsync($"api/profile/info");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<UserData>();
                return new ResponseModel<UserData>()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = content
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
        try
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
        catch(Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetPassword(string currentPassword, string newPassword)
    {
        try
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
        catch(Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetLanguage(int language)
    {
        try
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
        catch(Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions()
    {
        try
        {
            var response = await Client.GetAsync("api/profile/permissions");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<PermissionsModel>();
                return new ResponseModel<PermissionsModel>()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = content
                };
            }

            return new ResponseModel<PermissionsModel>()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel<PermissionsModel>()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    // ---------------------------- SETTINGS ----------------------------

    public async Task<ResponseModel<SettingsModel>> GetSettings()
    {
        try
        {
            var response = await Client.GetAsync("api/settings/");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<SettingsModel>();
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = content
                };
            }

            return new ResponseModel<SettingsModel>()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetSystemName(string newSystemName)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("api/settings/systemname", new SystemNameModel() { SystemName = newSystemName });
            if(response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };


        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetSystemTime(string newSystemTime)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("api/settings/systemtime", new SystemTimeModel() { TimeZoneCode = newSystemTime });
            if(response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetTimeBetweenUpdates(int newTimeBetweenUpdates)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("api/settings/timebetweenupdates", new TimeBetweenUpdatesModel() { TimeBetweenUpdatesSeconds = newTimeBetweenUpdates });
            if(response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    // ---------------------------- Permissions ----------------------------

    public async Task<ResponseModel<Dictionary<string, List<string>>>> GetAllPermissions()
    {
        try
        {
            var response = await Client.GetAsync("api/permissions/all");
            if (response.IsSuccessStatusCode)
                return new ResponseModel<Dictionary<string, List<string>>>()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = await response.Content.ReadFromJsonAsync<Dictionary<string, List<string>>>()
                };
            return new ResponseModel<Dictionary<string, List<string>>>()
            {
                Success = false,
                Message = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception e)
        {
            return new ResponseModel<Dictionary<string, List<string>>>()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
            };
        }
    }

    public async Task<ResponseModel<PermissionsModel>> GetPermissionsForUser(string user)
    {
        try
        {
            var response = await Client.GetAsync($"api/permissions/user/{user}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<PermissionsModel>();
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = content
                };
            }

            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetPermissionsForUser(string user, AddPermissionsModel model)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/permissions/user/{user}", model);
            if (response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch(Exception e)
        {
            return new()
            {
                Exception = e,
                Success = false,
                StatusCode = 500,
                Message = e.Message

            };
        }
    }

    // TODO IMPLEMENT ROLE PERMISSIONS -> NOT YET NEEDED

    // ---------------------------- USER MANAGEMENT ----------------------------

    public async Task<ResponseModel<List<UsersModel>>> GetAllUsers()
    {
        try
        {
            var response = await Client.GetAsync("api/users/all");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<List<UsersModel>>();
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = content
                };
            }

            return new ResponseModel<List<UsersModel>>()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel<UsersModel>> GetSpecificUserInfo(string user)
    {
        try
        {
            var response = await Client.GetAsync($"api/users/{user}");
            if (response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode,
                    Response = await response.Content.ReadFromJsonAsync<UsersModel>()
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> AddNewUser(AddUserModel model)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("api/users/add", model);
            if (response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> DeleteUser(string user)
    {
        try
        {
            var response = await Client.DeleteAsync($"api/users/{user}");
            if (response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel()
            {
                Exception = e,
                StatusCode = 500,
                Success = false,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetUsernameForUser(string user, string newUsername)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/users/{user}/username", new ChangeUsernameModel() { Username = newUsername });
            if(response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetPasswordForUser(string user, string newPassword)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/users/{user}/password", new ChangePasswordModel() { NewPassword = newPassword });
            if(response.IsSuccessStatusCode)
                return new()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetIsActiveForUser(string user, bool isActive)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/users/{user}/isactive", new ChangeIsActiveModel() { IsActive = isActive });
            if (response.IsSuccessStatusCode)
                return new ResponseModel()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new ResponseModel()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetLanguageForUser(string user, int language)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/users/{user}/language", new ChangeLanguageModel() { Language = (Language) language });
            if (response.IsSuccessStatusCode)
                return new ResponseModel()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new ResponseModel()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }

    public async Task<ResponseModel> SetIsDarkModeForUser(string user, bool useDarkMode)
    {
        try
        {
            var response = await Client.PostAsJsonAsync($"api/users/{user}/isdarkmode", new UseDarkModeModel() { UseDarkMode = useDarkMode });
            if (response.IsSuccessStatusCode)
                return new ResponseModel()
                {
                    Success = true,
                    StatusCode = (int)response.StatusCode
                };
            return new ResponseModel()
            {
                Success = false,
                StatusCode = (int)response.StatusCode,
                Message = await response.Content.ReadAsStringAsync()
            };
        }
        catch (Exception e)
        {
            return new ResponseModel()
            {
                Success = false,
                StatusCode = 500,
                Exception = e,
                Message = e.Message
            };
        }
    }


}


