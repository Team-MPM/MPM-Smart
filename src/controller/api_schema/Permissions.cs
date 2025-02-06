namespace ApiSchema;

public record AddPermissionsModel(List<string> Permissions);
public record AddRolePermissions(List<string> Permissions);
public record RolePermissionModel(string RoleName);
