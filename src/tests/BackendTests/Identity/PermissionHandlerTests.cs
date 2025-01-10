using System.Security.Claims;
using Backend.Services.Identity;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using PluginBase.Services.Permissions;
using Xunit;

namespace BackendTests.Identity;

public class PermissionHandlerTests
{
    [Theory]
    [InlineData("permission", "permission")]
    [InlineData("permission.read", "permission.read")]
    [InlineData("permission.*", "permission.read")]
    [InlineData("permission.x.*", "permission.x.read")]
    [InlineData("permission.x.y.*", "permission.x.y.read")]
    public void HandleRequirement_Succeeds_WhenUserHasPermission(string permission, string requirement)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("Permissions", permission)
        ]));
        
        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(requirement)],
            user,
            null);

        var handler = new PermissionHandler();

        // Act
        handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("permission", "permission.read")]
    [InlineData("permission.x", "permission.y")]
    [InlineData("permission.x.y.z", "permission.a.b.c")]
    [InlineData("permission.dev1", "permission.dev1.read")]
    [InlineData("permission.x.*", "permission.y")]
    [InlineData("permission.x.1", "permission.x.2")]
    public void HandleRequirement_Fails_WhenUserDoesNotHavePermission(string permission, string requirement)
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim("Permissions", permission)
        ]));
        
        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement(requirement)],
            user,
            null);

        var handler = new PermissionHandler();

        // Act
        handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
    
    [Fact]
    public void HandleRequirement_Fails_WhenUserHasNoPermissions()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        
        var context = new AuthorizationHandlerContext(
            [new PermissionRequirement("permission")],
            user,
            null);

        var handler = new PermissionHandler();

        // Act
        handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}