using System.Runtime.InteropServices;
using System.Security.Claims;
using Backend.Services.Identity;
using Backend.Services.Permissions;
using Data.System;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using PluginBase.Services.Permissions;
using Xunit;

namespace BackendTests.Identity;

public class PermissionVerifierTests
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

        var handler = new PermissionVerifier();

        // Act
        handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Theory]
    [InlineData("permission.read", "permission.read", true)]
    [InlineData("permission.*", "permission.read", true)]
    [InlineData("permission.x.*", "permission.x.read", true)]
    [InlineData("permission.x.y.*", "permission.x.y.read", true)]
    [InlineData("permission.x.y.z", "permission.x.y.x", false)]
    [InlineData("permission.*", "*", false)]
    [InlineData("*", "permission.x.y.x", true)]
    [InlineData("temperature.*", "permission.*", false)]
    [InlineData("temperature.x.y", "permission.x.z", false)]

    public void CheckHasAccess(string permission, string requiredPermission, bool expectedResult)
    {
        var handler = new PermissionHandler();

        var result = handler.HasAccess(permission, requiredPermission);
        Assert.Equal(expectedResult, result);
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

        var handler = new PermissionVerifier();

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

        var handler = new PermissionVerifier();

        // Act
        handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}