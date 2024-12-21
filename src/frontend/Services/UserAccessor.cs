using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Frontend.Services;

public class UserAccessor
{
    private readonly ControllerConnectionManager m_ConnectionManager;
    public ClaimsPrincipal? User { get; private set; }

    public UserAccessor(ControllerConnectionManager connectionManager)
    {
        m_ConnectionManager = connectionManager;
        m_ConnectionManager.OnChange += OnChange;
    }

    private void OnChange()
    {
        var token = m_ConnectionManager.Token;
        if (token is null) return;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        User = new ClaimsPrincipal(new ClaimsIdentity(jwt.Claims, "Bearer"));
    }

    public bool IsAuthenticated() => User?.Identity?.IsAuthenticated ?? false;

    public string? GetUsername() => GetClaimValue("unique_name");

    public bool HasRole(string role) => User?.IsInRole(role) ?? false;

    public bool HasClaim(string claimType, string claimValue) => User?.HasClaim(claimType, claimValue) ?? false;

    public string? GetClaimValue(string claimType) => User?.FindFirst(claimType)?.Value;

    public IEnumerable<Claim>? GetClaims() => User?.Claims;

    public bool IsAdmin() => GetUsername() == "admin"; // TODO: @Olaf

    public bool HasPermission(string permission) => throw new NotImplementedException(); // TODO: @Olaf
}