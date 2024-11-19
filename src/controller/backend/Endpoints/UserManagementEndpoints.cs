using ApiSchema.Settings;
using ApiSchema.Usermanagement;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Endpoints;

public static class UserManagementEndpoints
{
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/usermanagement");

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

            if(user.UserName == "admin")
                return Results.BadRequest("Username for the \"admin\" user cannot be changed");

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

            if(model.NewPassword != model.ConfirmPassword)
                return Results.BadRequest("Passwords do not match");

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

    }
}