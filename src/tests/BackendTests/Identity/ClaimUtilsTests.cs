using System.Security.Claims;
using Backend.Utils;
using Data.System;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace BackendTests.Identity;

public class ClaimUtilsTests
{
     [Fact]
    public async Task GetAllUserClaims_Should_Return_Filtered_Claims()
    {
        // Arrange
        var userManagerMock = new Mock<UserManager<SystemUser>>(
            Mock.Of<IUserStore<SystemUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

        var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(), null!, null!, null!, null!);

        var user = new SystemUser();

        var userClaims = new List<Claim>
        {
            new Claim("Permissions", "Permission.Read.*"),
            new Claim("Permissions", "Permission.Write.Docs"),
            new Claim("Permissions", "Permission.Admin")
        };

        var userRoles = new List<string> { "Admin", "Editor" };

        var roleClaims = new List<Claim>
        {
            new Claim("Permissions", "Permission.Write.*"),
            new Claim("Permissions", "Permission.Read.Docs"),
            new Claim("Permissions", "Permission.Admin")
        };

        userManagerMock
            .Setup(u => u.GetClaimsAsync(user))
            .ReturnsAsync(userClaims);

        userManagerMock
            .Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(userRoles);

        roleManagerMock
            .Setup(r => r.FindByNameAsync("Admin"))
            .ReturnsAsync(new IdentityRole("Admin"));

        roleManagerMock
            .Setup(r => r.FindByNameAsync("Editor"))
            .ReturnsAsync(new IdentityRole("Editor"));

        roleManagerMock
            .Setup(r => r.GetClaimsAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(roleClaims);

        // Act
        var result = (await ClaimUtils.GetAllUserClaims(userManagerMock.Object, user, roleManagerMock.Object))
            .ToList();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Select(c => c.Value).Should().BeEquivalentTo(new List<string>
        {
            "Permission.Read.*", "Permission.Write.*", "Permission.Admin"
        });
    }
}