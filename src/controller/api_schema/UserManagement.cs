using ApiSchema.Enums;

namespace ApiSchema;

public record AddUserModel(string Username, string Password);
public record ChangeIsActiveModel(bool IsActive);
public record ChangeLanguageModel(Language Language);
public record ChangePasswordModel(string? NewPassword);
public record ChangeTimeZoneModel(string TimeZoneCode);
public record ChangeUsernameModel(string Username);
public record LanguageModel(string Language);
public record PasswordModel(string CurrentPassword, string NewPassword);
public record RemoveUserModel(string Username);
public record UseDarkModeModel(bool UseDarkMode);
public record UserData
{
    public required string Username { get; set; }
    public required Language Language  { get; set; }
    public required bool UseDarkMode  { get; set; }
    public required List<string> Permissions  { get; set; }
    public required List<string> Roles  { get; set; }
    public required Dictionary<string, List<string>> RolePermissions  { get; set; }
}
public record UsernameModel(string UserName);
public record UsersModel
{
    public required string Username { get; set; }
    public required bool CanChangeUsername { get; set; }
    public required bool IsActive { get; set; }
    public required string Language { get; set; }
    public required bool UseDarkMode { get; set; }
    public required List<string> Permissions { get; set; }
}

