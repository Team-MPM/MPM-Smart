using System.Security.Claims;
using ApiSchema.Identity;
using Backend.Extensions;
using Backend.Services.Identity;
using Data.System;
using LanguageExt;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/permissions");

        group.MapGet("allPermissions", ([FromServices] AvailablePermissionProvider provider) => provider.PermissionsList)
            .RequireAuthorization("token");

        group.MapGet("/permissions", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var user = userManager.GetUserAsync(context.User).Result;
            if (user is null)
                return Results.Unauthorized();
            var userPermissions = await userManager.GetClaimsAsync(user);

            var roles = await userManager.GetRolesAsync(user);
            Dictionary<string, IEnumerable<string>> roleClaims = new();
            foreach (var role in roles)
            {
                var claimlist = await roleManager.GetClaimsAsync(await roleManager.FindByNameAsync(role));
                roleClaims.Add(role, claimlist.Select(c => c.Value));
            }

            return Results.Ok(new PermissionsModel()
            {
                UserPermissions = userPermissions.Select(s => s.Value),
                RolePermissions = roleClaims
            });
        }).RequirePermission(UserClaims.ViewProfile);

        group.MapPost("/updateUserPermissions", async (
            UserManager<SystemUser>userManager,
            AddPermissionsModel model) =>
        {
            var user = await userManager.FindByNameAsync(model.UserUsername);
            if (user is null)
                return Results.BadRequest("User not found");

            await userManager.RemoveClaimsAsync(user, await userManager.GetClaimsAsync(user));

            foreach (var claim in model.Permissions)
            {
                await userManager.AddClaimAsync(user, new Claim("Permissions", claim));
            }

            return Results.Ok();
        }).RequirePermission(UserClaims.ChangeUserPermissions);

        group.MapPost("/addPermissionsToUser", async (
            UserManager<SystemUser> userManager,
            [FromBody] AddPermissionsModel model) =>
        {
            var user = await userManager.FindByNameAsync(model.UserUsername);
            if (user is null)
                return Results.BadRequest("User not found");

            var userClaims = await userManager.GetClaimsAsync(user);
            var claims = userClaims.Select(s => s.Value).ToList();

            claims = claims.Union(model.Permissions).ToList();
            foreach (var claim in claims)
            {
                if (userClaims.Any(s => s.Value == claim))
                    continue;
                await userManager.AddClaimAsync(user, new Claim("Permissions", claim));
            }

            return Results.Ok();

        }).RequirePermission(UserClaims.ChangeUserPermissions);

        group.MapPost("/removePermissionsForUser", async (
            UserManager<SystemUser> userManager,
            [FromBody] AddPermissionsModel model) =>
        {
            var user = await userManager.FindByNameAsync(model.UserUsername);
            if (user is null)
                return Results.BadRequest("User not found");

            var userClaims = await userManager.GetClaimsAsync(user);
            var claims = userClaims.Select(s => s.Value).ToList();

            claims = claims.Except(model.Permissions).ToList();
            foreach (var claim in claims)
            {
                if (!userClaims.Any(s => s.Value == claim))
                    continue;
                await userManager.RemoveClaimAsync(user, new Claim("Permissions", claim));
            }

            return Results.Ok();
        }).RequirePermission(UserClaims.ChangeUserPermissions);

        return endpoints;
    }
}