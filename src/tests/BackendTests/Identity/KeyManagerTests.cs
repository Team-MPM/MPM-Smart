using Backend.Utils;
using FluentAssertions;
using Xunit;

namespace BackendTests.Identity;

public class KeyManagerTests
{
    [Fact]
    public async Task CreateKeyIfNotExists_CreatesKey_WhenKeyDoesNotExist()
    {
        // Arrange
        var path = Path.GetTempFileName();
        if (File.Exists(path))
            File.Delete(path);

        // Act
        await KeyManager.CreateKeyIfNotExists(path);

        // Assert
        File.Exists(path).Should().BeTrue();
    }
    
    [Fact]
    public async Task CreateKeyIfNotExists_DoesNotCreateKey_WhenKeyExists()
    {
        // Arrange
        var path = Path.GetTempFileName();
        await KeyManager.CreateKeyIfNotExists(path);
        var key1 = await File.ReadAllTextAsync(path);

        // Act
        await KeyManager.CreateKeyIfNotExists(path);
        var key2 = await File.ReadAllTextAsync(path);

        // Assert
        File.Exists(path).Should().BeTrue();
        key2.Should().Be(key1);
    }
    
    [Fact]
    public async Task LoadKey_LoadsKey()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), "temp.rsa");
        await KeyManager.CreateKeyIfNotExists(path);

        // Act
        var key = await KeyManager.LoadKey(path);

        // Assert
        key.Should().NotBeNull();
        key.KeySize.Should().Be(2048);
    }
}