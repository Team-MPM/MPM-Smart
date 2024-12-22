namespace PluginBase.Services.Permissions;

public class AvailablePermissionProvider
{
    public Dictionary<string, List<string>> PermissionsList { get; set; }

    public AvailablePermissionProvider()
    {
        PermissionsList = new();
    }

    public void AddRange(string source, List<string> permissions)
    {
        PermissionsList.Add(source, permissions);
    }
}