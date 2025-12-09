using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Aloe.Apps.MedockLib.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.MedockServer.Tests.Services;

/// <summary>
/// JwtTokenServiceのテスト
/// </summary>
public class JwtTokenServiceTests
{
    private static IDateTimeProvider CreateDateTimeProvider()
    {
        return new JstDateTimeProvider();
    }

    private static JwtTokenService CreateService(JwtSettings? settings = null)
    {
        settings ??= new JwtSettings
        {
            SecretKey = "TestSecretKeyForJwtTokenServiceTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        };
        return new JwtTokenService(Options.Create(settings), CreateDateTimeProvider());
    }

    private static TokenGenerationParams CreateBasicParams(Guid? userId = null, string userCode = "testuser", string email = "test@example.com")
    {
        return new TokenGenerationParams
        {
            UserId = userId ?? Guid.NewGuid(),
            UserCode = userCode,
            Email = email
        };
    }

    [Fact]
    public void GenerateAccessToken_Should_Return_Valid_Token()
    {
        // Arrange
        var service = CreateService();
        var tokenParams = CreateBasicParams();

        // Act
        var token = service.GenerateAccessToken(tokenParams);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
    }

    [Fact]
    public void GenerateAccessToken_Should_Contain_OIDC_Claims()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var tokenParams = CreateBasicParams(userId);

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert - OIDC標準クレームを検証
        jwtToken.Claims.Should().Contain(c => c.Type == "sub" && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "preferred_username" && c.Value == "testuser");
        jwtToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateAccessToken_Should_Have_Correct_Expiration()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "TestSecretKeyForJwtTokenServiceTestingPurposes123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 30,
            RefreshTokenExpirationDays = 7
        };
        var service = CreateService(settings);
        var tokenParams = CreateBasicParams();

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var dateTimeProvider = CreateDateTimeProvider();
        var expectedExpiration = dateTimeProvider.UtcNow.AddMinutes(30);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateRefreshToken_Should_Return_Unique_Tokens()
    {
        // Arrange
        var service = CreateService();

        // Act
        var token1 = service.GenerateRefreshToken();
        var token2 = service.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateToken_Should_Return_Principal_For_Valid_Token()
    {
        // Arrange
        var service = CreateService();
        var userId = Guid.NewGuid();
        var tokenParams = CreateBasicParams(userId);
        var token = service.GenerateAccessToken(tokenParams);

        // Act
        var principal = service.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.FindFirst("sub")?.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Invalid_Token()
    {
        // Arrange
        var service = CreateService();
        var invalidToken = "invalid.token.here";

        // Act
        var principal = service.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_Should_Return_Null_For_Token_With_Wrong_Secret()
    {
        // Arrange - 異なるシークレットキーでトークンを生成
        var dateTimeProvider = CreateDateTimeProvider();
        var service1 = new JwtTokenService(Options.Create(new JwtSettings
        {
            SecretKey = "OriginalSecretKeyForJwtTokenServiceTesting123!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        }), dateTimeProvider);
        var tokenParams = CreateBasicParams();
        var token = service1.GenerateAccessToken(tokenParams);

        // 異なるシークレットキーで検証
        var service2 = new JwtTokenService(Options.Create(new JwtSettings
        {
            SecretKey = "DifferentSecretKeyForJwtTokenServiceTesting!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        }), dateTimeProvider);

        // Act
        var principal = service2.ValidateToken(token);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateAccessToken_With_Roles_Should_Include_Role_Claims()
    {
        // Arrange
        var service = CreateService();
        var tokenParams = new TokenGenerationParams
        {
            UserId = Guid.NewGuid(),
            UserCode = "testuser",
            Email = "test@example.com",
            Roles = new[] { "Admin", "User" }
        };

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "roles").ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Select(c => c.Value).Should().Contain("Admin");
        roleClaims.Select(c => c.Value).Should().Contain("User");
    }

    [Fact]
    public void GenerateAccessToken_With_TenantId_Should_Include_Tenant_Claim()
    {
        // Arrange
        var service = CreateService();
        var tenantId = Guid.NewGuid();
        var tokenParams = new TokenGenerationParams
        {
            UserId = Guid.NewGuid(),
            UserCode = "testuser",
            Email = "test@example.com",
            TenantId = tenantId
        };

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == "tenant_id" && c.Value == tenantId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_With_Facility_Should_Include_Facility_Claims()
    {
        // Arrange
        var service = CreateService();
        var facilityId = Guid.NewGuid();
        var tokenParams = new TokenGenerationParams
        {
            UserId = Guid.NewGuid(),
            UserCode = "testuser",
            Email = "test@example.com",
            FacilityId = facilityId,
            FacilityName = "テスト施設",
            DisplayName = "テストユーザー"
        };

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == "facility_id" && c.Value == facilityId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == "facility_name" && c.Value == "テスト施設");
        jwtToken.Claims.Should().Contain(c => c.Type == "display_name" && c.Value == "テストユーザー");
    }

    [Fact]
    public void GenerateAccessToken_With_AdminFlags_Should_Include_Admin_Claims()
    {
        // Arrange
        var service = CreateService();
        var tokenParams = new TokenGenerationParams
        {
            UserId = Guid.NewGuid(),
            UserCode = "admin",
            Email = "admin@example.com",
            IsSystemAdmin = true
        };

        // Act
        var token = service.GenerateAccessToken(tokenParams);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == "is_system_admin" && c.Value == "true");
    }
}
