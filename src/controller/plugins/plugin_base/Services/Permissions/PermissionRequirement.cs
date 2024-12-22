using Microsoft.AspNetCore.Authorization;

namespace PluginBase.Services.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}