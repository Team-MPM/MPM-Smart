using ApiSchema.Usermanagement;
using Backend.Extensions;
using Backend.Services.Identity;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            return Results.Ok(new UsersModel()
            {
                Username = userEntity.UserName!,
                CanChangeUsername = userEntity.CanChangeUsername,
                IsActive = userEntity.IsActive,
                Language = (int)userEntity.UserProfile!.Language,
                UseDarkMode = userEntity.UserProfile.UseDarkMode
            });
        }).RequirePermission(UserClaims.UserViewUsers);

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
                return Results.Forbid();
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
            var result = await userManager.AddPasswordAsync(userEntity, model.NewPassword);
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
                UserManager<SystemUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user is null)
                    return Results.Unauthorized();
                var users = await userManager.Users.Include(s => s.UserProfile).ToListAsync();
                var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
                return Results.Ok(users.Select(u => new UsersModel()
                {
                    Username = u.UserName!,
                    Language = (int) u.UserProfile!.Language,
                    UseDarkMode = u.UserProfile.UseDarkMode,
                    CanChangeUsername = u.CanChangeUsername,
                    IsActive = u.IsActive,
                }));
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
            var usersInAdmin = await userManager.GetUsersInRoleAsync("admin");
            if (!usersInAdmin.Contains(user))
                return Results.Forbid();
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


            return Results.Created();
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
            var username = context.Request.Query["username"];
            var userToRemove = await userManager.FindByNameAsync(user);
            if (userToRemove is null)
                return Results.NotFound();
            var result = await userManager.DeleteAsync(userToRemove);
            return result.Succeeded ? Results.Ok() : Results.InternalServerError();
        }).RequirePermission(UserClaims.UserRemoveUser);

        return endpoints;
    }
}