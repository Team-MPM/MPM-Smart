using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiSchema;
using ApiSchema.Enums;

namespace Frontend.Services;

public class ApiAccessor(ControllerConnectionManager controllerConnectionManager, IServiceProvider sp)
{
    public ControllerConnectionDetails? Details { get; set; }
    private HttpClient? Client => controllerConnectionManager.GetCurrentClient();

    private async Task<ResponseModel> GetResponseModel(Func<HttpClient, Task<HttpResponseMessage>> request)
    {
        if (Client is null)
            return new ResponseModel().NoClientError();

        try
        {
            var response = await request(Client);
            if (!response.IsSuccessStatusCode)
            {
                if (await RefreshAndCheck())
                    response = await request(Client);
            }

            return !response.IsSuccessStatusCode
                ? await new ResponseModel().ServerError(response)
                : await new ResponseModel().SuccessResultAsync(response);
        }
        catch (Exception e)
        {
            try
            {
                if (await RefreshAndCheck())
                    return await GetResponseModel(request);
            }
            catch (Exception)
            {
                /* I don't like this warning, it's useless */
            }

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
            if (!response.IsSuccessStatusCode)
            {
                if (await RefreshAndCheck())
                    response = await request(Client);
            }

            return !response.IsSuccessStatusCode
                ? await new ResponseModel<T>().ServerError(response)
                : await new ResponseModel<T>().SuccessResultAsync(response);
        }
        catch (Exception e)
        {
            try
            {
                if (await RefreshAndCheck())
                    return await GetResponseModel<T>(request);
            }
            catch (Exception)
            {
                /* I don't like this warning, it's useless */
            }

            return new ResponseModel<T>().ExceptionError(e);
        }
    }

    // ---------------------------- IDENTITY ----------------------------

    public async Task<ResponseModel> TryConnect() =>
        await GetResponseModel(client => client.GetAsync("/api/settings/tryconnect"));

    // ---------------------------- IDENTITY ----------------------------
    public async Task<ResponseModel<LoginResponse>> Login(string username, string password, TimeSpan duration = new())
    {
        if (duration.Duration() == TimeSpan.Zero)
            duration = TimeSpan.FromDays(2);
        var response = await GetResponseModel<LoginResponse>(client =>
            client.PostAsJsonAsync("/api/identity/login", new LoginModel(
                UserName: username, 
                Password: password,
                Duration: duration
            )));

        if (response.Success)
        {
            response.Response = new LoginResponse(
                response.Response!.Token.Replace("\"", ""),
                response.Response!.RefreshToken?.Replace("\"", "") ?? "");
        }

        return response;
    }

    public async Task<ResponseModel<string>> TryRefreshToken(TimeSpan duration = new())
    {
        if (duration.Duration() == TimeSpan.Zero)
            duration = TimeSpan.FromDays(2);
        using var scope = sp.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenHandler>();

        if (Details is null || Client is null)
            return new ResponseModel<string>().NoClientError();
        return await tokenService.RefreshTokenAsync(Client!, Details.Address, Details.Port.ToString(), duration);
    }

    // ---------------------------- PROFILE ----------------------------

    public async Task<ResponseModel<UserData>> GetUserData() =>
        await GetResponseModel<UserData>(client => client.GetAsync($"api/profile/info"));

    public async Task<ResponseModel> SetUsername(string newUsername) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/username",
            new UsernameModel(newUsername)));

    public async Task<ResponseModel> SetPassword(string currentPassword, string newPassword) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/password",
            new PasswordModel(
                CurrentPassword: currentPassword,
                NewPassword: newPassword
            )));

    public async Task<ResponseModel> SetLanguage(string language) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/profile/language",
            new LanguageModel(language)));

    public async Task<ResponseModel<PermissionsModel>> GetUserPermissions() =>
        await GetResponseModel<PermissionsModel>(client =>
            client.GetAsync("/api/profile/permissions"));

    // ---------------------------- SETTINGS ----------------------------

    public async Task<ResponseModel<SettingsModel>> GetSettings() =>
        await GetResponseModel<SettingsModel>(client => client.GetAsync("/api/settings/"));

    public async Task<ResponseModel> SetSystemName(string newSystemName) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/systemname",
            new SystemNameModel(newSystemName)));

    public async Task<ResponseModel> SetSystemTime(string newSystemTime) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/systemtime",
            new SystemTimeModel(newSystemTime)));

    public async Task<ResponseModel> SetTimeBetweenUpdates(int newTimeBetweenUpdates) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/timebetweenupdates",
            new TimeBetweenUpdatesModel(newTimeBetweenUpdates)));

    public async Task<ResponseModel<string>> GetTimeZone() =>
        await GetResponseModel<string>(client => client.GetAsync("/api/settings/timezone"));

    public async Task<ResponseModel> SetTimeZone(string timezoneCode) =>
        await GetResponseModel(client => client.PostAsJsonAsync("/api/settings/timezone",
            new ChangeTimeZoneModel(timezoneCode)));

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
            new ChangeUsernameModel(newUsername)));

    public async Task<ResponseModel> UpdateUser(string user, UsersModel model) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}", model));

    public async Task<ResponseModel> SetPasswordForUser(string user, string? newPassword) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/password",
            new ChangePasswordModel(newPassword)));

    public async Task<ResponseModel> SetIsActiveForUser(string user, bool isActive) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/isactive",
            new ChangeIsActiveModel(isActive)));

    public async Task<ResponseModel> SetLanguageForUser(string user, int language) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/language",
            new ChangeLanguageModel((Language)language)));

    public async Task<ResponseModel> SetIsDarkModeForUser(string user, bool useDarkMode) =>
        await GetResponseModel(client => client.PostAsJsonAsync($"/api/users/{user}/isdarkmode",
            new UseDarkModeModel(useDarkMode)));

    // ---------------------------- Plugins ----------------------------

    public async Task<ResponseModel<List<PluginInfoDto>>> GetAllPlugins() =>
        await GetResponseModel<List<PluginInfoDto>>(client => client.GetAsync("/api/plugins"));

    public async Task<ResponseModel<List<OptionsDto>>> GetPluginOptions(string pluginGuid) =>
        await GetResponseModel<List<OptionsDto>>(client => client.GetAsync($"/api/plugins/{pluginGuid}/options"));

    // ---------------------------- Devices ----------------------------

    public async Task<ResponseModel<List<DeviceDto>>> GetConnectedDevices() =>
        await GetResponseModel<List<DeviceDto>>(client => client.GetAsync("/api/devices"));

    public async Task<ResponseModel<List<DeviceInfoDto>>> ScanForDevices() =>
        await GetResponseModel<List<DeviceInfoDto>>(client => client.GetAsync("/api/devices/scan"));

    public async Task<ResponseModel<List<SensorDto>>> GetAllSensors() =>
        await GetResponseModel<List<SensorDto>>(client => client.GetAsync("/api/sensors"));

    public async Task<ResponseModel<DeviceDto>> GetDevice(string serial) =>
        await GetResponseModel<DeviceDto>(client => client.GetAsync($"/api/device/{serial}"));

    // ------------------------------ Data ------------------------------------
    
    public async Task<ResponseModel<List<DataPointDto>>> GetDataPoints() =>
        await GetResponseModel<List<DataPointDto>>(client => client.GetAsync($"/api/data"));
    
    // ------------------------------------------------------------------------
    
    private async Task<bool> RefreshAndCheck()
    {
        try
        {
            var tokenResponse = await TryRefreshToken();
            if (!tokenResponse.Success)
                return false;
            Client!.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.Response);
            var checkResult = await GetResponseModel(client => client.GetAsync("/api/identity/checkToken"));
            if (!checkResult.Success)
                return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}