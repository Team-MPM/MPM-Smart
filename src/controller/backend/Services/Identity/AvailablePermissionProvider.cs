using LanguageExt;

namespace Backend.Services.Identity;

public class AvailablePermissionProvider
{
    public List<string> PermissionsList { get; set; }

    public AvailablePermissionProvider()
    {
        PermissionsList = UserClaims.ExportPermissions().OrderBy(s => s).ToList();
    }

    private void AddRange(List<string> permissions)
    {
        PermissionsList.AddRange(permissions);
    }
}