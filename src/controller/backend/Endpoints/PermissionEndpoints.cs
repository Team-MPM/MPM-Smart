using System.Security.Claims;
using ApiSchema.Identity;
using ApiSchema.Permissions;
using Backend.Extensions;
using Backend.Services.Identity;
using Data.System;
using LanguageExt;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Services.Permissions;

namespace Backend.Endpoints;

public static class PermissionEndpoints
{
    public static IEndpointRouteBuilder MapPermissionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/permissions");

        group.MapGet("/all", ([FromServices] AvailablePermissionProvider provider) => provider.PermissionsList)
            .RequireAuthorization("token");

        group.MapGet("/user/{user}", async (
            [FromRoute] string user,
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var IdUser = await userManager.FindByNameAsync(user);
            if (IdUser is null)
                return Results.Unauthorized();
            var userPermissions = await userManager.GetClaimsAsync(IdUser);

            var roles = await userManager.GetRolesAsync(IdUser);
            Dictionary<string, IEnumerable<string>> roleClaims = new();
            foreach (var role in roles)
            {
                var claimList = await roleManager.GetClaimsAsync((await roleManager.FindByNameAsync(role))!);
                roleClaims.Add(role, claimList.Select(c => c.Value));
            }

            return Results.Ok(new PermissionsModel()
            {
                UserPermissions = userPermissions.Select(s => s.Value),
                RolePermissions = roleClaims
            });
        }).RequirePermission(UserClaims.ProfileViewProfile);

        group.MapPost("/user/{user}", async (
            [FromRoute] string user,
            UserManager<SystemUser>userManager,
            AddPermissionsModel model) =>
        {
            var idUser = await userManager.FindByNameAsync(user);
            if (idUser is null)
                return Results.NotFound("User not found");

            await userManager.RemoveClaimsAsync(idUser, await userManager.GetClaimsAsync(idUser));

            foreach (var claim in model.Permissions)
            {
                await userManager.AddClaimAsync(idUser, new Claim("Permissions", claim));
            }

            return Results.Ok();
        }).RequirePermission(UserClaims.PermissionsChangeUserPermissions);


        group.MapGet("/role/{role}", async (
            [FromRoute] string role,
            RoleManager<IdentityRole> roleManager) =>
        {
            var idRole = await roleManager.FindByNameAsync(role);
            if (idRole is null)
                return Results.NotFound("Role not found");
            var roles = await roleManager.GetClaimsAsync(idRole);
            return Results.Ok(roles.Select(s => s.Value));
        }).RequirePermission(UserClaims.PermissionsViewRolePermissions);

        group.MapPost("/role/{role}", async (
            [FromRoute] string role,
            RoleManager<IdentityRole> roleManager,
            [FromBody] AddRolePermissions model) =>
        {
            var idRole = await roleManager.FindByNameAsync(role);
            if (idRole is null)
                return Results.NotFound("Role not found");
            foreach (var claim in await roleManager.GetClaimsAsync(idRole))
                await roleManager.RemoveClaimAsync(idRole, claim);
            foreach (var claim in model.Permissions)
                await roleManager.AddClaimAsync(idRole, new Claim("Permissions", claim));
            return Results.Ok();
        }).RequirePermission(UserClaims.PermissionsChangeRolePermissions);

        // NOT needed, in case we actually need them, they need to be adjusted!
        // group.MapPost("/addPermissionsToUser", async (
        //     UserManager<SystemUser> userManager,
        //     [FromBody] AddPermissionsModel model) =>
        // {
        //     var user = await userManager.FindByNameAsync(model.UserUsername);
        //     if (user is null)
        //         return Results.NotFound("User not found");
        //
        //     var userClaims = await userManager.GetClaimsAsync(user);
        //     var claims = userClaims.Select(s => s.Value).ToList();
        //
        //     claims = claims.Union(model.Permissions).ToList();
        //     foreach (var claim in claims)
        //     {
        //         if (userClaims.Any(s => s.Value == claim))
        //             continue;
        //         await userManager.AddClaimAsync(user, new Claim("Permissions", claim));
        //     }
        //
        //     return Results.Ok();
        //
        // }).RequirePermission(UserClaims.PermissionsChangeUserPermissions);
        //
        // group.MapPost("/removePermissionsForUser", async (
        //     UserManager<SystemUser> userManager,
        //     [FromBody] AddPermissionsModel model) =>
        // {
        //     var user = await userManager.FindByNameAsync(model.UserUsername);
        //     if (user is null)
        //         return Results.NotFound("User not found");
        //
        //     var userClaims = await userManager.GetClaimsAsync(user);
        //     var claims = userClaims.Select(s => s.Value).ToList();
        //
        //     claims = claims.Except(model.Permissions).ToList();
        //     foreach (var claim in claims)
        //     {
        //         if (!userClaims.Any(s => s.Value == claim))
        //             continue;
        //         await userManager.RemoveClaimAsync(user, new Claim("Permissions", claim));
        //     }
        //
        //     return Results.Ok();
        // }).RequirePermission(UserClaims.PermissionsChangeUserPermissions);

        return endpoints;
    }
}