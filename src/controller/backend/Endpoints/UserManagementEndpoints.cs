using System.Reflection.Metadata.Ecma335;
using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using Data.System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Endpoints;

public static class UserManagementEndpoints
{
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/user");

        group.MapGet("/username", async (
            HttpContext context,
            UserManager<SystemUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
                return Results.Unauthorized();

            return Results.Ok(user.UserName!);

        }).RequireAuthorization("token");

        group.MapPost("/username", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            [FromBody] UsernameModel model) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();

            if(!user.CanChangeUsername)
                return Results.BadRequest("Username for the \"admin\" user cannot be changed"); //TODO

            if (string.IsNullOrWhiteSpace(model.Username))
                return Results.BadRequest();

            var result = await userManager.SetUserNameAsync(user, model.Username);

            return result.Succeeded ? Results.Ok() : Results.BadRequest(result.Errors);

        }).RequireAuthorization("token");

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

        }).RequireAuthorization("token");

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

        }).RequireAuthorization("token");

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
            userProfile!.Language = (Language) model.Language;
            await dbContext.SaveChangesAsync();
            return Results.Ok();
        }).RequireAuthorization("token");

        group.MapGet("/getUsers", async (
            HttpContext context,
            UserManager<SystemUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
                return Results.Unauthorized();
            var users = await userManager.Users.Include(s => s.UserProfile).ToListAsync();
            var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
            return Results.Ok(users.Select(u => new
            {
                Username=u.UserName,
                Language=u.UserProfile!.Language,
                UseDarkMode=u.UserProfile.UseDarkMode,
                IsAdmin=usersInAdmin.Contains(u)
            }));
        }).RequireAuthorization("token");

        group.MapPost("/addUser", async (
            HttpContext context,
            [FromBody] AddUserModel model,
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if(user is null)
                return Results.Unauthorized();
            var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
            if (!usersInAdmin.Contains(user))
                return Results.Forbid();
            var newUser = await userManager.FindByNameAsync(model.Username);
            if(newUser is not null)
                return Results.BadRequest("User already exists");

            var result = await userManager.CreateAsync(new SystemUser()
            {
                UserName = model.Username,
                UserProfile = new UserProfileEntity()
            }, model.Password);

            if (!result.Succeeded)
                return Results.InternalServerError();

            return Results.Created();
        }).RequireAuthorization("token");

        group.MapDelete("/removeUser", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            [FromBody] RemoveUserModel model) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if(user is null)
                return Results.Unauthorized();
            var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
            if (!usersInAdmin.Contains(user))
                return Results.Forbid();
            var username = context.Request.Query["username"];
            var userToRemove = await userManager.FindByNameAsync(model.Username);
            if(userToRemove is null)
                return Results.NotFound();
            var result = await userManager.DeleteAsync(userToRemove);
            return result.Succeeded ? Results.Ok() : Results.InternalServerError();
        }).RequireAuthorization("token");
    }
}