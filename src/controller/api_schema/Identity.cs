namespace ApiSchema;

public record LoginModel(string UserName, string Password);

public record PermissionsModel(
    IEnumerable<string> UserPermissions,
    Dictionary<string, IEnumerable<string>> RolePermissions);

public record AssignRoleModel(string Username, string Role);
