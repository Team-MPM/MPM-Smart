using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiSchema.Identity;
using Data.System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder endpoints, RsaSecurityKey key)
    {
        var group = endpoints.MapGroup("/api/identity");

        group.MapPost("/login", async (
            [FromBody] LoginModel model,
            [FromServices] UserManager<SystemUser> userManager,
            HttpContext context) =>
        {
            const string errorMessage = "Invalid username or password";
            
            var user = await userManager.FindByNameAsync(model.UserName);
            
            if (user is null)
                return Results.BadRequest(errorMessage);
            
            if (!await userManager.CheckPasswordAsync(user, model.Password))
                return Results.BadRequest(errorMessage);
            
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity([
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName!),
                    //new Claim("role", user)
                ]),
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
        }).RequireAuthorization("token");
    }
}