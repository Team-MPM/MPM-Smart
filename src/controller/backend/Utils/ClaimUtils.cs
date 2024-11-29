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
        var userClaimsTask = userManager.GetClaimsAsync(user);
        var userRolesTask = userManager.GetRolesAsync(user);

        await Task.WhenAll(userClaimsTask, userRolesTask);

        var userClaims = userClaimsTask.Result;
        var userRoles = userRolesTask.Result;

        var roleTasks = userRoles.Select(roleManager.FindByNameAsync).ToArray();
        await Task.WhenAll(roleTasks);

        var roles = roleTasks
            .Where(t => t.Result != null)
            .Select(t => t.Result!);

        var roleClaimsTasks = roles
            .Select(roleManager.GetClaimsAsync)
            .ToArray();

        await Task.WhenAll(roleClaimsTasks);

        var roleClaims = roleClaimsTasks
            .SelectMany(t => t.Result);

        var allClaims = userClaims.Select(c => c.Value).Union(roleClaims.Select(c => c.Value))
            .ToHashSet();

        foreach (var claim in allClaims.Where(c => c.Contains('*')).ToList())
        {
            var prefix = claim.Replace("*", string.Empty);
            allClaims.RemoveWhere(c => c != claim && c.StartsWith(prefix));
        }

        return allClaims.Select(c => new Claim("Permissions", c));
    }
}