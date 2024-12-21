using System.Net.Http.Json;
using ApiSchema.Enums;
using ApiSchema.Identity;
using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using PermissionsModel = ApiSchema.Usermanagement.PermissionsModel;

namespace Frontend.Services;

public class ApiAccessor(ControllerConnectionManager controllerConnectionManager)
{
    private HttpClient? Client => controllerConnectionManager.GetCurrentClient();

    private async Task<ResponseModel> GetResponseModel(Func<HttpClient, Task<HttpResponseMessage>> request)
    {
        if (Client is null)
            return new ResponseModel().NoClientError();

        try
        {
            var response = await request(Client);
            return !response.IsSuccessStatusCode
                ? new ResponseModel().ServerError(response)
                : await new ResponseModel().SuccessResultAsync(response);
        }
        catch (Exception e)
        {
            return new ResponseModel().ExceptionError(e);
        }
    }

    private async Task<ResponseModel<T>> GetResponseModel<T>(Func<HttpClient, Task<HttpResponseMessage>> request)
        where T : class
    {
        if (Client is null)
            return new ResponseModel<T>().NoClientError();

        try
        {
            var response = await request(Client);
            return !response.IsSuccessStatusCode
                ? new ResponseModel<T>().ServerError(response)
                : await new ResponseModel<T>().SuccessResultAsync(response);
        }
        catch (Exception e)
        {
            return new ResponseModel<T>().ExceptionError(e);
        }
    }

    // ---------------------------- IDENTITY ----------------------------
    public async Task<ResponseModel<string>> Login(string username, string password)
    {
        var response = await GetResponseModel<string>(client =>
            client.PostAsJsonAsync("/api/identity/login", new LoginModel
            {
                UserName = username, Password = password
            }));

        if (response.Success)
            response.Response = response.Response!.Trim('\"');
        return response;
    }

    // ---------------------------- PROFILE ----------------------------

    public async Task<ResponseModel<UserData>> GetUserData() =>
        await GetResponseModel<UserData>(client => client.GetAsync($"api/profile/info"));

    public async Task<ResponseModel> SetUsername(string newUsername) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/username",
            new UsernameModel
            {
                Username = newUsername
            }));

    public async Task<ResponseModel> SetPassword(string currentPassword, string newPassword) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/password",
            new PasswordModel
            {
                CurrentPassword = currentPassword, NewPassword = newPassword
            }));

    public async Task<ResponseModel> SetLanguage(int language) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/language",
            new LanguageModel
            {
                Language = language
            }));

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions() =>
        await GetResponseModel<PermissionsModel>(client =>
            client.GetAsync("api/profile/permissions"));

    // ---------------------------- SETTINGS ----------------------------

    public async Task<ResponseModel<SettingsModel>> GetSettings() =>
        await GetResponseModel<SettingsModel>(client => client.GetAsync("api/settings/"));

    public async Task<ResponseModel> SetSystemName(string newSystemName) =>
        await GetResponseModel(client => client.PostAsJsonAsync("api/settings/systemname",
            new SystemNameModel
            {
                SystemName = newSystemName
            }));

    public async Task<ResponseModel> SetSystemTime(string newSystemTime) =>
        await GetResponseModel(client => client.PostAsJsonAsync("api/settings/systemtime",
            new SystemTimeModel
            {
                TimeZoneCode = newSystemTime
            }));

    public async Task<ResponseModel> SetTimeBetweenUpdates(int newTimeBetweenUpdates) =>
        await GetResponseModel(client => client.PostAsJsonAsync("api/settings/timebetweenupdates",
            new TimeBetweenUpdatesModel
            {
                TimeBetweenUpdatesSeconds = newTimeBetweenUpdates
            }));

    // ---------------------------- Permissions ----------------------------

    public async Task<ResponseModel<Dictionary<string, List<string>>>> GetAllPermissions() =>
        await GetResponseModel<Dictionary<string, List<string>>>(client =>
            client.GetAsync("api/permissions/all"));

    public async Task<ResponseModel<PermissionsModel>> GetPermissionsForUser(string user) =>
        await GetResponseModel<PermissionsModel>(client =>
            client.GetAsync($"api/permissions/user/{user}"));

    public async Task<ResponseModel> SetPermissionsForUser(string user, AddPermissionsModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/permissions/user/{user}", model));

    // TODO: IMPLEMENT ROLE PERMISSIONS -> NOT YET NEEDED

    // ---------------------------- USER MANAGEMENT ----------------------------

    public async Task<ResponseModel<List<UsersModel>>> GetAllUsers() =>
        await GetResponseModel<List<UsersModel>>(client => client.GetAsync("api/users/all"));

    public async Task<ResponseModel<UsersModel>> GetSpecificUserInfo(string user) =>
        await GetResponseModel<UsersModel>(client => client.GetAsync($"api/users/{user}"));

    public async Task<ResponseModel> AddNewUser(AddUserModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync("api/users/add", model));

    public async Task<ResponseModel> DeleteUser(string user) =>
        await GetResponseModel(client => client.DeleteAsync($"api/users/{user}"));

    public async Task<ResponseModel> SetUsernameForUser(string user, string newUsername) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/users/{user}/username",
            new ChangeUsernameModel
            {
                Username = newUsername
            }));

    public async Task<ResponseModel> SetPasswordForUser(string user, string newPassword) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/users/{user}/password",
            new ChangePasswordModel
            {
                NewPassword = newPassword
            }));

    public async Task<ResponseModel> SetIsActiveForUser(string user, bool isActive) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/users/{user}/isactive",
            new ChangeIsActiveModel
            {
                IsActive = isActive
            }));

    public async Task<ResponseModel> SetLanguageForUser(string user, int language) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/users/{user}/language",
            new ChangeLanguageModel
            {
                Language = (Language)language
            }));

    public async Task<ResponseModel> SetIsDarkModeForUser(string user, bool useDarkMode) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"api/users/{user}/isdarkmode",
            new UseDarkModeModel
            {
                UseDarkMode = useDarkMode
            }));
}