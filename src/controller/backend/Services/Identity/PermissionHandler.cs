namespace Backend.Services.Identity;

using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User
            .FindAll(c => c.Type == "Permissions")
            .Select(s => s.Value);

        var requiredPermissionParts = requirement.Permission.AsSpan().Split('.');

        foreach (var permission in userPermissions)
        {
            // Check for the exact match
            if (permission == requirement.Permission)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check for wildcard match
            if (!permission.Contains('*'))
                continue;

            var permissionSpan = permission.AsSpan();
            foreach (var requiredPermissionPart in requiredPermissionParts)
            {
                if (permissionSpan.StartsWith('*'))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
                
                var requirementPermissionPart = requirement.Permission.AsSpan(requiredPermissionPart);

                if (!permissionSpan.StartsWith(requirementPermissionPart))
                    break;
                
                permissionSpan = permissionSpan[(requirementPermissionPart.Length + 1)..];
            }
        }

        return Task.CompletedTask;
    }
}