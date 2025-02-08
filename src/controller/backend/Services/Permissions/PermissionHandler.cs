using System.Security.Claims;
using Data.System;
using LanguageExt.Effects.Traits;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services.Permissions;

public class PermissionHandler()
{
    public bool HasAccess(ClaimsPrincipal claimsPrincipal, string requiredPermission)
    {
        var permissions = claimsPrincipal.Claims.Select(c => c.Value).ToList();
        if (!permissions.Any())
            return false;
        return HasAccess(permissions, requiredPermission);
    }

    public bool HasAccess(string permission, string requiredPermission) =>
        HasAccess(new List<string>() { permission }, requiredPermission);

    public bool HasAccess(List<string> permissions, string requiredPermission)
    {
        if (permissions.Any(p => p == requiredPermission || p == "*"))
            return true;

        var requiredPermissionParts = requiredPermission.Split('.');

        permissions = permissions.Where(p => p.Contains('*'))
            .Where(p => p.Split('.').Length <= requiredPermissionParts.Length)
            .Where(p => p.Split('.')[0] == requiredPermissionParts[0]).ToList();

        foreach (var permission in permissions)
        {
            var permissionParts = permission.Split('.');
            for (int i = 0; i < permissionParts.Length; i++)
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