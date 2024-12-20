using ApiSchema.Enums;

namespace BackendConnectionData.Model;

public class UserData
{
    public required string Username { get; set; }
    public required Language Language  { get; set; }
    public required bool UseDarkMode  { get; set; }
    public required List<string> Permissions  { get; set; }
    public required List<string> Roles  { get; set; }
    public required Dictionary<string, List<string>> RolePermissions  { get; set; }
}
