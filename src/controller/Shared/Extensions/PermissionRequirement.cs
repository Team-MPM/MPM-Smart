using Microsoft.AspNetCore.Authorization;

namespace Backend.Services.Identity;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}