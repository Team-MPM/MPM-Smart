using ApiSchema;
using ApiSchema.Enums;
using Backend.Services.Identity;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PluginBase.Services.Permissions;
namespace Backend.Endpoints;

public static class UserProfileEndpoints
{
    public static void MapUserProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/profile");

        group.MapGet("/info", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var user =  await userManager.Users.
                Include(u => u.UserProfile)
                .FirstOrDefaultAsync(s => s.UserName == context.User.Identity!.Name);
            if(user is null)
                return Results.Unauthorized();
            var userPermissions = await userManager.GetClaimsAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            Dictionary<string, List<string>> roleClaims = new();
            foreach (var role in roles)
            {
                var roleItem = await roleManager.FindByNameAsync(role);
                if (roleItem is not null)
                {
                    var claims = await roleManager.GetClaimsAsync(roleItem);
                    var claimStrings = claims.Select(s => s.Value).ToList();
                    roleClaims.Add(role, claimStrings.ToList());
                }

            }

            return Results.Ok(new
            {
                Username = user.UserName,
                Language = user.UserProfile!.Language,
                UseDarkMode = user.UserProfile.UseDarkMode,
                Permissions = userPermissions.Select(s => s.Value),
                Roles = roles,
                RolePermissions = roleClaims
            });

        }).RequirePermission(UserClaims.ProfileViewInfo);

        group.MapGet("/username", async (
            HttpContext context,
            UserManager<SystemUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
                return Results.Unauthorized();

            return Results.Ok(user.UserName!);

        }).RequirePermission(UserClaims.ProfileViewProfile);

        group.MapPost("/username", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            [FromBody] UsernameModel model) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();

            if(!user.CanChangeUsername)
                return Results.BadRequest($"Username for the '{user.UserName}' user cannot be changed");

            if (string.IsNullOrWhiteSpace(model.UserName))
                return Results.BadRequest();

            var result = await userManager.SetUserNameAsync(user, model.UserName);

            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);

        }).RequirePermission(UserClaims.ProfileChangeUsername);

        group.MapPost("/password", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            [FromBody] PasswordModel model) =>
        {
            var user = userManager.GetUserAsync(context.User).Result;
            if (user is null)
                return Results.Unauthorized();

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            return result == IdentityResult.Success ? Results.Ok() : Results.BadRequest(result.Errors);

        }).RequirePermission(UserClaims.ProfileChangePassword);

        group.MapGet("/backup", () =>
        {
            // TODO: Implement backup functionality
        }).RequireAuthorization("token");

        group.MapGet("/language", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            SystemDbContext dbContext) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();
            var userProfile = await dbContext.UserProfiles.FindAsync(user.UserProfileId);
            return Results.Ok(userProfile!.Language.ToString());

        }).RequirePermission(UserClaims.ProfileViewProfile);

        group.MapPost("/language", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            SystemDbContext dbContext,
            [FromBody] LanguageModel model) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();
            if(!Enum.IsDefined(typeof(Language), model.Language))
                return Results.BadRequest("Invalid language");

            var userProfile = await dbContext.UserProfiles.FindAsync(user.UserProfileId);
            userProfile!.Language = Enum.Parse<Language>(model.Language);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }).RequirePermission(UserClaims.ProfileEditProfile);

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
                var claimlist = await roleManager.GetClaimsAsync((await roleManager.FindByNameAsync(role))!);
                roleClaims.Add(role, claimlist.Select(c => c.Value));
            }

            return Results.Ok(new PermissionsModel(
                UserPermissions: userPermissions.Select(s => s.Value),
                RolePermissions: roleClaims
            ));
        }).RequirePermission(UserClaims.ProfileViewProfile);
    }
}