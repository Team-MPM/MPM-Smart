using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiSchema.Identity;
using Backend.Extensions;
using Backend.Utils;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Backend.Services.Identity;

namespace Backend.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder endpoints, RsaSecurityKey key)
    {
        var group = endpoints.MapGroup("/api/identity");

        group.MapPost("/login", async (
            [FromBody] LoginModel model,
            [FromServices] UserManager<SystemUser> userManager,
            [FromServices] RoleManager<IdentityRole> roleManager) =>
        {
            const string errorMessage = "Invalid username or password";

            var user = await userManager.FindByNameAsync(model.UserName);

            if (user is null)
                return Results.BadRequest(errorMessage);

            if (!await userManager.CheckPasswordAsync(user, model.Password))
                return Results.BadRequest(errorMessage);

            var handler = new JwtSecurityTokenHandler();

            var claims = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            ]);

            claims.AddClaims(await ClaimUtils.GetAllUserClaims(userManager, user, roleManager));

            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            });

            var tokenString = handler.WriteToken(token);
            return Results.Ok(tokenString);
        });

        group.MapGet("/profile", async (
            HttpContext context,
            UserManager<SystemUser> userManager,
            SystemDbContext dbContext) =>
        {
            var user = await userManager.GetUserAsync(context.User);

            if (user is null)
                return Results.Unauthorized();

            var profile = await dbContext.UserProfiles.FindAsync(user.UserProfileId);

            return Results.Ok(new
            {
                User = user.UserName,
                Profile = profile as UserProfile
            });
        }).RequirePermission(UserClaims.ViewProfile);


        // TODO OPTIONAL


        // group.MapGet("/logout", async (
        //     HttpContext context,
        //     UserManager<SystemUser> userManager,
        //     SystemDbContext dbContext) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //
        //     if (user is null)
        //         return Results.Unauthorized();
        //
        //     var token = context.Request.Headers["Authorization"].ToString().Split(" ")[1];
        //
        //     return Results.Ok();
        //
        // }).RequireAuthorization("token");
    }
}