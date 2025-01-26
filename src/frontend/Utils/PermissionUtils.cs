namespace Frontend.Utils;

public static class PermissionUtils
{
    public static bool CheckIfDisabled(string permission, Dictionary<string, bool> permissions)
    {
        var highest = GetHighestJokerPermission(permission.Split('.').Length, permission, permissions);
        return !string.IsNullOrEmpty(highest) &&
               highest != permission &&
               permission.Contains(highest.Replace(".*", "").Replace("*", ""));
    }

    public static string? GetHighestJokerPermission(int permissionsLength, string permissionType, Dictionary<string, bool> permissions)
    {
        if (!(permissionType.EndsWith(".*") || permissionType.EndsWith("*")))
        {
            var splittedPermission = permissionType.Split(".").ToList();
            splittedPermission.RemoveAt(splittedPermission.Count - 1);
            permissionType = string.Join(".", splittedPermission);
        }

        var highest = permissions.Where(s => s.Value)
            .Select(s => s.Key)
            .Where(s => s.Contains('*'))
            .Where(s => s.Replace(".*", "").Split('.').Length <= permissionsLength)
            .Where(s => s.Split('.').First() == permissionType.Split('.').First() || s.Split('.').First() == "*")
            .OrderBy(p => p.Split('.').Length)
            .ThenBy(p => p.Length)
            .ToList();

        if (highest.Count <= 1)
            return highest.FirstOrDefault();
        
        return highest.Any(s => s.Split(".").Length < permissionsLength) 
            ? highest.FirstOrDefault(s => s.Split(".").Length < permissionsLength) 
            : highest.FirstOrDefault(s => s.Contains(permissionType));
    }
}