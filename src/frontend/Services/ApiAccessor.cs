using System.Net.Http.Json;
using ApiSchema.Devices;
using ApiSchema.Enums;
using ApiSchema.Identity;
using ApiSchema.Plugins;
using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using Blazored.LocalStorage;
using Frontend.Pages.General;
using Shared.Plugins.DataInfo;
using Shared.Plugins.DataRequest;
using Shared.Plugins.DataResponse;
using PermissionsModel = ApiSchema.Usermanagement.PermissionsModel;

namespace Frontend.Services;

public class ApiAccessor(ControllerConnectionManager controllerConnectionManager, IServiceProvider sp)
{
    public ControllerConnectionDetails Details { get; set; }
    private HttpClient? Client => controllerConnectionManager.GetCurrentClient();

    private async Task<ResponseModel> GetResponseModel(Func<HttpClient, Task<HttpResponseMessage>> request)
    {
        if (Client is null)
            return new ResponseModel().NoClientError();

        try
        {
            var response = await request(Client);
            if ((int)response.StatusCode == 401)
            {
                var tokenResponse = await TryRefreshToken();
                if(tokenResponse.Success)
                    response = await request(Client);
            }
            return !response.IsSuccessStatusCode
                ? await new ResponseModel().ServerError(response)
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
            if ((int)response.StatusCode == 401)
            {
                var tokenResponse = await TryRefreshToken();
                if(tokenResponse.Success)
                    response = await request(Client);
            }
            return !response.IsSuccessStatusCode
                ? await new ResponseModel<T>().ServerError(response)
                : await new ResponseModel<T>().SuccessResultAsync(response);
        }
        catch (Exception e)
        {
            return new ResponseModel<T>().ExceptionError(e);
        }
    }

    // ---------------------------- IDENTITY ----------------------------
    public async Task<ResponseModel<LoginResponse>> Login(string username, string password, int duration = 2, LoginDurationEntity durationEntity = LoginDurationEntity.Minute)
    {
        var response = await GetResponseModel<LoginResponse>(client =>
            client.PostAsJsonAsync("/api/identity/login", new LoginModel
            {
                UserName = username, Password = password,
                LoginDuration = duration,
                LoginDurationEntity = durationEntity
            }));

        if (response.Success)
        {
            response.Response!.Token = response.Response!.Token.Replace("\"", "");
            if(!string.IsNullOrEmpty(response.Response.RefreshToken))
                response.Response!.RefreshToken = response.Response!.RefreshToken.Replace("\"", "");
        }
        return response;
    }
    public async Task<ResponseModel<string>> TryRefreshToken()
    {
        using var scope = sp.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenHandler>();

        return await tokenService.RefreshTokenAsync(Client!, Details.Address, Details.Port.ToString());
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

    public async Task<ResponseModel> SetLanguage(string language) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/language",
            new LanguageModel
            {
                Language = language
            }));

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions() =>
        await GetResponseModel<PermissionsModel>(client =>
            client.GetAsync("/api/profile/permissions"));

    // ---------------------------- SETTINGS ----------------------------

    public async Task<ResponseModel<SettingsModel>> GetSettings() =>
        await GetResponseModel<SettingsModel>(client => client.GetAsync("/api/settings/"));

    public async Task<ResponseModel> SetSystemName(string newSystemName) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/systemname",
            new SystemNameModel
            {
                SystemName = newSystemName
            }));

    public async Task<ResponseModel> SetSystemTime(string newSystemTime) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/systemtime",
            new SystemTimeModel
            {
                TimeZoneCode = newSystemTime
            }));

    public async Task<ResponseModel> SetTimeBetweenUpdates(int newTimeBetweenUpdates) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/timebetweenupdates",
            new TimeBetweenUpdatesModel
            {
                TimeBetweenUpdatesSeconds = newTimeBetweenUpdates
            }));

    public async Task<ResponseModel<string>> GetTimeZone() =>
        await GetResponseModel<string>(client => client.GetAsync("/api/settings/timezone"));

    public async Task<ResponseModel> SetTimeZone(string timezoneCode) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/timezone",
            new ChangeTimeZoneModel()
            {
                TimeZoneCode = timezoneCode
            }));

    public async Task<ResponseModel<Dictionary<string, double>>> GetTimeZones() =>
        await GetResponseModel<Dictionary<string, double>>(client => client.GetAsync("/api/settings/timeZones"));

    // ---------------------------- Permissions ----------------------------

    public async Task<ResponseModel<Dictionary<string, List<string>>>> GetAllPermissions() =>
        await GetResponseModel<Dictionary<string, List<string>>>(client =>
            client.GetAsync("/api/permissions/all"));

    public async Task<ResponseModel<PermissionsModel>> GetPermissionsForUser(string user) =>
        await GetResponseModel<PermissionsModel>(client =>
            client.GetAsync($"/api/permissions/user/{user}"));

    public async Task<ResponseModel> SetPermissionsForUser(string user, AddPermissionsModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/permissions/user/{user}", model));

    // TODO: IMPLEMENT ROLE PERMISSIONS -> NOT YET NEEDED

    // ---------------------------- USER MANAGEMENT ----------------------------

    public async Task<ResponseModel<List<UsersModel>>> GetAllUsers() =>
        await GetResponseModel<List<UsersModel>>(client => client.GetAsync("/api/users"));

    public async Task<ResponseModel<UsersModel>> GetSpecificUserInfo(string user) =>
        await GetResponseModel<UsersModel>(client => client.GetAsync($"/api/users/{user}"));

    public async Task<ResponseModel> AddNewUser(AddUserModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/users/", model));

    public async Task<ResponseModel> DeleteUser(string user) =>
        await GetResponseModel(client => client.DeleteAsync($"/api/users/{user}"));

    public async Task<ResponseModel> SetUsernameForUser(string user, string newUsername) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/username",
            new ChangeUsernameModel
            {
                Username = newUsername
            }));
    public async Task<ResponseModel> UpdateUser(string user, UsersModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}", model));

    public async Task<ResponseModel> SetPasswordForUser(string user, string? newPassword) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/password",
            new ChangePasswordModel
            {
                NewPassword = newPassword
            }));

    public async Task<ResponseModel> SetIsActiveForUser(string user, bool isActive) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/isactive",
            new ChangeIsActiveModel
            {
                IsActive = isActive
            }));

    public async Task<ResponseModel> SetLanguageForUser(string user, int language) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/language",
            new ChangeLanguageModel
            {
                Language = (Language)language
            }));

    public async Task<ResponseModel> SetIsDarkModeForUser(string user, bool useDarkMode) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/isdarkmode",
            new UseDarkModeModel
            {
                UseDarkMode = useDarkMode
            }));

    // ---------------------------- Plugins ----------------------------

    public async Task<ResponseModel<List<PluginInfoDto>>> GetAllPlugins() =>
        await GetResponseModel<List<PluginInfoDto>>(client => client.GetAsync("/api/plugins"));

    public async Task<ResponseModel<List<OptionsDto>>> GetPluginOptions(string pluginGuid) =>
        await GetResponseModel<List<OptionsDto>>(client => client.GetAsync($"/api/plugins/{pluginGuid}/options"));

    // ---------------------------- PluginData ----------------------------

    public async Task<ResponseModel<DataInfoResponse>> GetPluginDataInfo() =>
        await GetResponseModel<DataInfoResponse>(client => client.GetAsync("/api/data/info"));

    public async Task<ResponseModel<DataResponse>> GetPluginData(DataRequest request) =>
        await GetResponseModel<DataResponse>(client => client.PostAsJsonAsync("/api/data/requestData", request));

    // ---------------------------- Devices ----------------------------

    public async Task<ResponseModel<List<DeviceDto>>> GetConnectedDevices() =>
        await GetResponseModel<List<DeviceDto>>(client => client.GetAsync("/api/devices"));

    public async Task<ResponseModel<List<DeviceInfoDto>>> ScanForDevices() =>
        await GetResponseModel<List<DeviceInfoDto>>(client => client.GetAsync("/api/devices/scan"));

    public async Task<ResponseModel<List<SensorDto>>> GetAllSensors() =>
        await GetResponseModel<List<SensorDto>>(client => client.GetAsync("/api/sensors"));

    public async Task<ResponseModel<DeviceDto>> GetDevice(string serial) =>
        await GetResponseModel<DeviceDto>(client => client.GetAsync($"/api/device/{serial}"));
}