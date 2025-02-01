namespace ApiSchema;

public record LoginModel(string UserName, string Password, TimeSpan Duration);

public record LoginResponse(string Token, string RefreshToken);

public record RefreshTokenModel(string RefreshToken, TimeSpan Duration);

public record PermissionsModel(
    IEnumerable<string> UserPermissions,
    Dictionary<string, IEnumerable<string>> RolePermissions);

public record AssignRoleModel(string Username, string Role);
