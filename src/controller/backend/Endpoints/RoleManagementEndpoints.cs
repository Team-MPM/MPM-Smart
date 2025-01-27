using ApiSchema;
using Backend.Services.Identity;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PluginBase.Services.Permissions;

namespace Backend.Endpoints;

public static class RoleManagementEndpoints
{
    public static IEndpointRouteBuilder MapRoleManagementEndpoint(this IEndpointRouteBuilder endpoint)
    {
        var group = endpoint.MapGroup("/api/roles");

        group.MapGet("/", async (
            RoleManager<IdentityRole> roleManager) =>
        {
            var roles = await roleManager.Roles.ToListAsync();
            return roles.Select(s => s.Name);
        }).RequirePermission(UserClaims.RoleViewRoles);

        group.MapGet("/role/{name}", async (
            [FromRoute] string name,
            RoleManager<IdentityRole> roleManager) =>
        {
            var role = await roleManager.FindByNameAsync(name);
            if (role is null)
                return Results.NotFound();
            var claims = await roleManager.GetClaimsAsync(role);
            return Results.Ok(new
            {
                Id = role.Id,
                Name= role.Name,
                Permissions = claims.Select(s => s.Value)
            });
        }).RequirePermission(UserClaims.RoleViewRoles);

        group.MapPost("/role/{name}", async (
        [FromRoute] string name,
            RoleManager<IdentityRole> roleManager) =>
        {
            var role = await roleManager.FindByNameAsync(name);
            if (role is not null)
                return Results.BadRequest("Role already exists");
            var result = await roleManager.CreateAsync(new IdentityRole(name));
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }).RequirePermission(UserClaims.RoleManageRoles);

        group.MapDelete("/role/{name}", async (
            [FromRoute] string name,
            RoleManager<IdentityRole> roleManager) =>
        {
            var role = await roleManager.FindByNameAsync(name);
            if (role is null)
                return Results.NotFound();
            if(role.Name!.ToLower() == "admin")
                return Results.BadRequest("Cannot delete admin role");
            var result = await roleManager.DeleteAsync(role);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }).RequirePermission(UserClaims.RoleManageRoles);

        group.MapPost("/assignRole", async (
            RoleManager<IdentityRole> roleManager,
            UserManager<SystemUser> userManager,
            [FromBody] AssignRoleModel model) =>
        {
            var role = await roleManager.FindByNameAsync(model.Role);
            if (role is null)
                return Results.NotFound("Role not found");
            var user = await userManager.FindByNameAsync(model.Username);
            if (user is null)
                return Results.NotFound("User not found");
            var result = await userManager.AddToRoleAsync(user, role.Name!);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }).RequirePermission(UserClaims.RoleAssignUsers);

        group.MapDelete("/assignRole", async (
            RoleManager<IdentityRole> roleManager,
            UserManager<SystemUser> userManager,
            [FromBody] AssignRoleModel model) =>
        {
            var role = await roleManager.FindByNameAsync(model.Role);
            if (role is null)
                return Results.NotFound("Role not found");
            var user = await userManager.FindByNameAsync(model.Username);
            if (user is null)
                return Results.NotFound("User not found");
            if(user.UserName!.ToLower() == "admin" && role.Name!.ToLower() == "admin")
                return Results.BadRequest("Cannot remove admin role from admin user");
            var result = await userManager.RemoveFromRoleAsync(user, role.Name!);
            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);
        }).RequirePermission(UserClaims.RoleAssignUsers);

        return endpoint;
    }
}