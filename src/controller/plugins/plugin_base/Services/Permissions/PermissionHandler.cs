using System.Security.Claims;

namespace PluginBase.Services.Permissions;

public static class PermissionHandler
{
    public static bool HasAccess(ClaimsPrincipal claimsPrincipal, string requiredPermission)
    {
        var permissions = claimsPrincipal.Claims.Select(c => c.Value).ToList();
        return HasAccess(permissions, requiredPermission);
    }
    
    public static bool HasAccess(List<Claim> claims, string requiredPermission)
    {
        var permissions = claims.Select(c => c.Value).ToList();
        return HasAccess(permissions, requiredPermission);
    }
    
    public static bool HasAccess(string permission, string requiredPermission) =>
        HasAccess([permission], requiredPermission);

    public static bool HasAccess(List<string> permissions, string requiredPermission)
    {
        if (permissions.Any(p => p == requiredPermission || p == "*"))
            return true;

        var requiredPermissionParts = requiredPermission.Split('.');

        permissions = permissions.Where(p => p.Contains('*'))
            .Where(p => p.Split('.').Length <= requiredPermissionParts.Length)
            .Where(p => p.Split('.')[0] == requiredPermissionParts[0]).ToList();

        foreach (var permissionParts in permissions.Select(permission => permission.Split('.')))
        {
            for (var i = 0; i < permissionParts.Length; i++)
            {
                if(permissionParts[i] == "*")
                    return true;
                if(permissionParts[i] != requiredPermissionParts[i])
                    break;
            }
        }
        return false;
    }
}