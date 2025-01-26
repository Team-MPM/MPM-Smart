using System.Security.Claims;
using ApiSchema.Enums;
using ApiSchema.Usermanagement;
using Backend.Services.Identity;
using Data.System;
using LanguageExt.ClassInstances;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PluginBase.Services.Permissions;

namespace Backend.Endpoints;

public static class UserManagementEndpoints
{
    public static IEndpointRouteBuilder MapUserManagementEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api");


        group.MapGet("/users/{user}", async (
            UserManager<SystemUser> userManager,
            string user) =>
        {
            var userEntity = await userManager.Users
                .Include(s => s.UserProfile).FirstOrDefaultAsync(s => s.UserName == user);
            if (userEntity is null)
                return Results.NotFound();
            var userPermissions = await userManager.GetClaimsAsync(userEntity);
            return Results.Ok(new UsersModel
            {
                Username = userEntity.UserName!,
                CanChangeUsername = userEntity.CanChangeUsername,
                IsActive = userEntity.IsActive,
                Language = userEntity.UserProfile!.Language.ToString(),
                UseDarkMode = userEntity.UserProfile.UseDarkMode,
                Permissions = userPermissions.Select(s => s.Value).ToList(),
            });
        }).RequirePermission(UserClaims.UserViewUsers);

        group.MapPost("/users/{user}", async (
            UserManager<SystemUser> userManager,
            [FromServices] SystemDbContext dbContext,
            string user,
            [FromBody] UsersModel model) =>
        {
            var errorMessage = "";
            var userEntity = await dbContext.Users
                .Include(s => s.UserProfile).FirstOrDefaultAsync(s => s.UserName == user);
            if(user != model.Username && await dbContext.Users.AnyAsync(s => s.UserName == model.Username))
                errorMessage += "Username already exists";
            if (userEntity is null)
                return Results.NotFound();
            if(userEntity.CanChangeUsername)
                userEntity.UserName = model.Username;
            if (userEntity.UserName != "admin")
                userEntity.CanChangeUsername = model.CanChangeUsername;
            userEntity.IsActive = model.IsActive;
            var claims = await userManager.GetClaimsAsync(userEntity);
            claims.ToList().ForEach(c => userManager.RemoveClaimAsync(userEntity, c));
            model.Permissions.ForEach(p => userManager.AddClaimAsync(userEntity, new Claim("Permissions", p)));
            var parsable = Enum.TryParse<Language>(model.Language.ToString(), out var language);
            if(parsable)
                userEntity.UserProfile!.Language = language;
            else
                errorMessage += errorMessage == "" ? "Language not found" : ", Language not found";
            userEntity.UserProfile!.UseDarkMode = model.UseDarkMode;
            if(errorMessage != "")
                return Results.BadRequest(errorMessage);
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }).RequirePermission(UserClaims.UserChangeUserUsername);

        group.MapPost("/users/{user}/username", async (
            [FromRoute] string user,
            [FromBody] ChangeUsernameModel model,
            SystemDbContext context) =>
        {
            var userEntity = await context.Users
                .Include(s => s.UserProfile).FirstOrDefaultAsync(s => s.UserName == user);
            if (userEntity is null)
                return Results.NotFound("User not found");
            if (!userEntity.CanChangeUsername)
                return Results.Unauthorized();
            userEntity.UserName = model.Username;
            var result = await context.SaveChangesAsync();
            return result == 1 ? Results.Ok() : Results.InternalServerError();
        }).RequirePermission(UserClaims.UserChangeUserUsername);

        group.MapPost("/users/{user}/password", async (
            UserManager<SystemUser> userManager,
            [FromRoute] string user,
            [FromBody] ChangePasswordModel model) =>
        {
            var userEntity = await userManager.FindByNameAsync(user);
            if (userEntity is null)
                return Results.NotFound("User not found");
            await userManager.RemovePasswordAsync(userEntity);
            var result = await userManager.AddPasswordAsync(userEntity, model.NewPassword!);
            return result == IdentityResult.Success ? Results.Ok() : Results.BadRequest(result.Errors);
        }).RequirePermission(UserClaims.UserChangeUserPassword);

        group.MapPost("/users/{user}/isactive", async (
            [FromRoute] string user,
            [FromBody] ChangeIsActiveModel model,
            SystemDbContext context) =>
        {
            var userEntity = context.Users.FirstOrDefault(s => s.UserName == user);
            if (userEntity is null)
                return Results.NotFound();
            if(userEntity.UserName == "admin")
                return Results.Forbid();
            userEntity.IsActive = model.IsActive;
            var result = await context.SaveChangesAsync();
            return result == 1 ? Results.Ok() : Results.InternalServerError();
        }).RequirePermission(UserClaims.UserChangeUserUsername);

        group.MapPost("/users/{user}/language", async (
            [FromRoute] string user,
            [FromBody] ChangeLanguageModel model,
            SystemDbContext dbContext) =>
        {
            var userEntity = await dbContext.Users
                .Include(s => s.UserProfile).FirstOrDefaultAsync(s => s.UserName == user);
            if (userEntity is null)
                return Results.NotFound();
            userEntity.UserProfile!.Language = model.Language;
            var result = await dbContext.SaveChangesAsync();
            return result == 1 ? Results.Ok() : Results.InternalServerError();
        }).RequirePermission(UserClaims.UserChangeUserUsername);

        group.MapPost("/users/{user}/useDarkMode", async (
            [FromRoute] string user,
            [FromBody] UseDarkModeModel model,
            SystemDbContext context) =>
        {
            var userEntity = await context.Users
                .Include(s => s.UserProfile).FirstOrDefaultAsync(s => s.UserName == user);
            if (userEntity is null)
                return Results.NotFound();
            userEntity.UserProfile!.UseDarkMode = model.UseDarkMode;
            var result = await context.SaveChangesAsync();
            return result == 1 ? Results.Ok() : Results.InternalServerError(result);
        }).RequirePermission(UserClaims.UserChangeUserUsername);

        group.MapGet("/users", async (
                HttpContext context,
                UserManager<SystemUser> userManager,
                RoleManager<IdentityRole> roleManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                    return Results.Unauthorized();
                var users = await userManager.Users.Include(s => s.UserProfile).ToListAsync();
                Dictionary<string, List<string>> permissions = new Dictionary<string, List<string>>();
                users.ForEach(u => permissions.Add(u.UserName!, userManager.GetClaimsAsync(u).Result.Select(s => s.Value).ToList()));
                List<UsersModel> allUsers = new();
                users.ForEach(u => allUsers.Add(new()
                {
                    Username = u.UserName!,
                    CanChangeUsername = u.CanChangeUsername,
                    IsActive = u.IsActive,
                    Language = u.UserProfile!.Language.ToString(),
                    UseDarkMode = u.UserProfile.UseDarkMode,
                    Permissions = permissions[u.UserName!]
                }));
                return Results.Ok(allUsers);
            }).RequirePermission(UserClaims.UserViewUsers);

        group.MapPost("/users", async (
            HttpContext context,
            [FromBody] AddUserModel model,
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();
            var newUser = await userManager.FindByNameAsync(model.Username);
            if (newUser is not null)
                return Results.BadRequest("User already exists");

            var result = await userManager.CreateAsync(new SystemUser()
            {
                UserName = model.Username,
                UserProfile = new UserProfileEntity(),
                IsActive = true,
                CanChangeUsername = true
            }, model.Password);

            if (!result.Succeeded)
                return Results.InternalServerError();


            return Results.Created(user.UserName, user);
        }).RequirePermission(UserClaims.UserAddUser);

        group.MapDelete("/users/{user}", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            [FromRoute] string user) =>
        {
            var userEntity = await userManager.GetUserAsync(context.User);
            if (userEntity is null)
                return Results.Unauthorized();
            var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
            if (!usersInAdmin.Contains(userEntity))
                return Results.Forbid();
            var userToRemove = await userManager.FindByNameAsync(user);
            if (userToRemove is null)
                return Results.NotFound();
            List<string> blockesUsernames = new () { "admin", "Visitor" };
            if (blockesUsernames.Contains(userToRemove.UserName!))
                return Results.Unauthorized();
            var result = await userManager.DeleteAsync(userToRemove);
            return result.Succeeded ? Results.Ok() : Results.InternalServerError();
        }).RequirePermission(UserClaims.UserRemoveUser);

        return endpoints;
    }
}