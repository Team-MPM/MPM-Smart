using System.Security.Claims;

namespace ApiSchema.Identity;

public class PermissionsModel
{
    public required IEnumerable<string> UserPermissions { get; set; }
    public required Dictionary<string, IEnumerable<string>> RolePermissions { get; set; }
}