using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static class RoleManagementEndpoints
{
    public static IEndpointRouteBuilder MapRoleManagementEndpoint(this IEndpointRouteBuilder endpoint)
    {
        var group = endpoint.MapGroup("/api/roles");

        group.MapGet("/all", async (
            RoleManager<IdentityRole> roleManager) =>
        {
            var roles = await roleManager.Roles.ToListAsync();
            return roles.Select(s => s.Name);
        });

        return endpoint;
    }
}