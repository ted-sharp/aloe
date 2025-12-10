using Aloe.Apps.MedockLib.Data.Entities;
using FluentAssertions;

namespace Aloe.Apps.MedockServer.Tests.Data.Entities;

/// <summary>
/// Userエンティティのテスト
/// </summary>
public class UserEntityTests
{
    [Fact]
    public void User_Should_Have_Required_Properties()
    {
        // Arrange & Act
        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserCode = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        // Assert
        user.UserId.Should().NotBeEmpty();
        user.UserCode.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hash");
        user.PasswordSalt.Should().Be("salt");
    }

    [Fact]
    public void User_Should_Have_Default_Values()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.IsSystemAdmin.Should().BeFalse();
        user.IsDeleted.Should().BeFalse();
        user.LoginSuccessCount.Should().Be(0);
        user.LoginFailureCount.Should().Be(0);
        user.LoginFailureAttempts.Should().Be(0);
    }

    [Fact]
    public void User_Should_Implement_IAuditableEntity()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Should().BeAssignableTo<IAuditableEntity>();
    }
}





