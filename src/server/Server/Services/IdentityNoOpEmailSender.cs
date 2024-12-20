using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Server.Data;

namespace Server.Services;

internal sealed class IdentityNoOpEmailSender : IEmailSender<ServerUser>
{
    private readonly NoOpEmailSender m_EmailSender = new();

    public Task SendConfirmationLinkAsync(ServerUser user, string email, string confirmationLink) =>
        m_EmailSender.SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ServerUser user, string email, string resetLink) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ServerUser user, string email, string resetCode) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password",
            $"Please reset your password using the following code: {resetCode}");
}