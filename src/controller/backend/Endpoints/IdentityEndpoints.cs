using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiSchema.Identity;
using Backend.Utils;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Backend.Services.Identity;
using PluginBase.Services.Permissions;

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

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (!await userManager.CheckPasswordAsync(user, model.Password))
                    return Results.BadRequest(errorMessage);
            }

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
                Expires = DateTime.UtcNow.AddMinutes(model.LoginDurationEntity switch
                {
                    LoginDurationEntity.Minute => model.LoginDuration!.Value,
                    LoginDurationEntity.Hour => model.LoginDuration!.Value * 60,
                    LoginDurationEntity.Day => model.LoginDuration!.Value * 60 * 24,
                    LoginDurationEntity.Month => model.LoginDuration!.Value * 60 * 24 * 30,
                    LoginDurationEntity.Year => model.LoginDuration!.Value * 60 * 24 * 365,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            });
            var tokenString = handler.WriteToken(token);

            var refreshTokenClaims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("type", "refresh_token"),
            });

            var refreshToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = refreshTokenClaims,
                TokenType = "refresh_token",
                Expires = DateTime.UtcNow.AddDays(30),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            });
            var refreshTokenString = handler.WriteToken(refreshToken);

            return Results.Ok(new LoginResponse()
            {
                Token = tokenString,
                RefreshToken = refreshTokenString
            });
        });
        group.MapPost("/refresh", async (
            UserManager<SystemUser> userManager,
            RoleManager<IdentityRole> roleManager,
            [FromBody] RefreshTokenModel model) =>
        {
            model.RefreshToken = model.RefreshToken.Trim('"');
            var token = new JwtSecurityToken(model.RefreshToken);
            var tokenUser = token.Claims.FirstOrDefault(s => s.Type == "nameid");



            if ((token.Claims.FirstOrDefault(s => s.Type == "type" && s.Value == "refresh_token") is null) ||
                tokenUser is null)
            {
                Console.WriteLine("Token" + token.Claims.FirstOrDefault(s => s.Type == "type")?.Value);
                Console.WriteLine("User:" + tokenUser?.Value);
                return Results.BadRequest("Invalid refresh token 1");
            }
            var user = await userManager.FindByIdAsync(tokenUser.Value);
            if (user is null)
                return Results.Unauthorized();
            if (token.ValidTo < DateTime.UtcNow)
                return Results.BadRequest("Refresh token expired");
            if (tokenUser.Value != user.Id)
                return Results.BadRequest("Invalid refresh token 2");

            var handler = new JwtSecurityTokenHandler();

            var claims = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            ]);

            claims.AddClaims(await ClaimUtils.GetAllUserClaims(userManager, user, roleManager));
            var newToken = handler.CreateToken(new SecurityTokenDescriptor()
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(model.DurationType switch
                {
                    LoginDurationEntity.Minute => model.Duration!.Value,
                    LoginDurationEntity.Hour => model.Duration!.Value * 60,
                    LoginDurationEntity.Day => model.Duration!.Value * 60 * 24,
                    LoginDurationEntity.Month => model.Duration!.Value * 60 * 24 * 30,
                    LoginDurationEntity.Year => model.Duration!.Value * 60 * 24 * 365,
                    _ => throw new ArgumentOutOfRangeException()
                }),
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
            });
            var newTokenString = handler.WriteToken(newToken);
            return Results.Ok(newTokenString);
        });

        // group.MapGet("/profile", async (
        //     HttpContext context,
        //     UserManager<SystemUser> userManager,
        //     SystemDbContext dbContext) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //
        //     if (user is null)
        //         return Results.Unauthorized();
        //
        //     var profile = await dbContext.UserProfiles.FindAsync(user.UserProfileId);
        //
        //     return Results.Ok(new
        //     {
        //         User = user.UserName,
        //         Profile = profile as UserProfile
        //     });
        // }).RequirePermission(UserClaims.ProfileViewProfile);


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