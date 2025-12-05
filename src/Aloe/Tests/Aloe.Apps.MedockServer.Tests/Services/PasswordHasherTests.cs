using Aloe.Apps.MedockLib.Services;
using FluentAssertions;

namespace Aloe.Apps.MedockServer.Tests.Services;

/// <summary>
/// PasswordHasherのテスト
/// </summary>
public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_Should_Return_Hash_And_Salt()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        salt.Should().NotBeNullOrEmpty();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void HashPassword_Should_Generate_Different_Salts()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = hasher.HashPassword(password);
        var (hash2, salt2) = hasher.HashPassword(password);

        // Assert
        salt1.Should().NotBe(salt2);
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "TestPassword123!";
        var (hash, salt) = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Wrong_Password()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var (hash, salt) = hasher.HashPassword(password);

        // Act
        var result = hasher.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Wrong_Salt()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "TestPassword123!";
        var (hash, _) = hasher.HashPassword(password);
        var (_, wrongSalt) = hasher.HashPassword("AnotherPassword");

        // Act
        var result = hasher.VerifyPassword(password, hash, wrongSalt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Default_Instance_Should_Have_Recommended_Settings()
    {
        // Arrange & Act
        var hasher = PasswordHasher.Default;

        // Assert
        hasher.SaltSize.Should().BeGreaterThanOrEqualTo(16);
        hasher.HashSize.Should().BeGreaterThanOrEqualTo(32);
        hasher.Iterations.Should().BeGreaterThanOrEqualTo(10000);
    }

    [Fact]
    public void HashPassword_Should_Work_With_Empty_Password()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = String.Empty;

        // Act
        var (hash, salt) = hasher.HashPassword(password);
        var result = hasher.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Should_Work_With_Unicode_Password()
    {
        // Arrange
        var hasher = PasswordHasher.Default;
        var password = "パスワード123!日本語";

        // Act
        var (hash, salt) = hasher.HashPassword(password);
        var result = hasher.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }
}

