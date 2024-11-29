using System.Security.Claims;
using Data.System;
using Microsoft.AspNetCore.Identity;

namespace Backend.Utils;

public static class ClaimUtils
{
    public static async Task<IEnumerable<Claim>> GetAllUserClaims(
        UserManager<SystemUser> userManager,
        SystemUser user,
        RoleManager<IdentityRole> roleManager)
    {
        var userClaimsString = userManager.GetClaimsAsync(user).Result.Select(s => s.Value);
        var userRoles = await userManager.GetRolesAsync(user);
        var roleClaims = userRoles
            .SelectMany(s => roleManager.GetClaimsAsync(roleManager.FindByNameAsync(s).Result!).Result)
            .Select(s => s.Value);
        var allClaims = userClaimsString.Union(roleClaims).ToList();
        IEnumerable<string> filteredClaims = new List<string>();

        foreach (var claim in from claim in allClaims
                 let splittedClaim = claim.Split('.')
                 from part in splittedClaim
                 where part == "*" select claim)
        {
            allClaims = allClaims
                .Where(s => !s.Contains(claim.Replace("*", "")) || s == claim)
                .ToList();
        }

        return allClaims.Select(c => new Claim("Permissions", c));
    }
}