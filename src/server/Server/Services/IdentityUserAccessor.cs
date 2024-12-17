using Microsoft.AspNetCore.Identity;
using Server.Data;

namespace Server.Services;

internal sealed class IdentityUserAccessor(
    UserManager<ServerUser> userManager,
    IdentityRedirectManager redirectManager)
{
    public async Task<ServerUser> GetRequiredUserAsync(HttpContext? context)
    {
        if (context is null)
            redirectManager.RedirectTo("Account/Login");

        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);

        return user;
    }
}