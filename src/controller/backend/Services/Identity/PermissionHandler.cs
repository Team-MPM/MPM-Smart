namespace Backend.Services.Identity;

using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        var userPermissions = context.User
            .FindAll(c => c.Type == "Permissions")
            .Select(s => s.Value)
            .ToList();
        
        if (userPermissions.Count == 0)
        {
            return Task.CompletedTask;
        }
        
        var requiredPermissionTypes = requirement.Permission.Split('.');

        foreach (var permission in userPermissions)
        {
            if(permission == requirement.Permission)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (!permission.Contains('*'))
                continue;

            for (var i = 0; i < requiredPermissionTypes.Length; i++)
            {
                var permissionParts = permission.Split('.');
                if (permissionParts[i] == "*")
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                if (permissionParts[i] != requiredPermissionTypes[i])
                    break;
            }
        }

        return Task.CompletedTask;
    }
}

