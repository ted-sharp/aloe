using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.MedockServer.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly MedockDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // InMemory Database
        var options = new DbContextOptionsBuilder<MedockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new MedockDbContext(options);

        // PasswordHasher
        _passwordHasher = PasswordHasher.Default;

        // JwtTokenService
        var jwtSettings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };
        _jwtTokenService = new JwtTokenService(Options.Create(jwtSettings));

        // AuthService
        _authService = new AuthService(_context, _passwordHasher, _jwtTokenService);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(User user, Tenant tenant)> SeedTestUserAsync(string userCode = "testuser", string password = "testpass")
    {
        var (hash, salt) = _passwordHasher.HashPassword(password);

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            TenantName = "テストテナント",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Tenants.Add(tenant);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserCode = userCode,
            Email = $"{userCode}@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsSystemAdmin = false,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.Users.Add(user);

        var tenantUser = new TenantUser
        {
            TenantUserId = Guid.NewGuid(),
            TenantId = tenant.TenantId,
            UserId = user.UserId,
            DisplayName = "テストユーザー",
            IsTenantAdmin = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.TenantUsers.Add(tenantUser);

        await _context.SaveChangesAsync();

        return (user, tenant);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var (user, tenant) = await SeedTestUserAsync("admin", "admin");

        // Act
        var result = await _authService.LoginAsync("admin", "admin", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.UserId);
        result.UserCode.Should().Be("admin");
        result.TenantId.Should().Be(tenant.TenantId);
        result.SessionId.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUserCode_ShouldFail()
    {
        // Arrange
        await SeedTestUserAsync("admin", "admin");

        // Act
        var result = await _authService.LoginAsync("wronguser", "admin", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.AccessToken.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        await SeedTestUserAsync("admin", "admin");

        // Act
        var result = await _authService.LoginAsync("admin", "wrongpassword", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.AccessToken.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ShouldFail()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("deleteduser", "password");
        user.IsDeleted = true;
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.LoginAsync("deleteduser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithLockedUser_ShouldFail()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("lockeduser", "password");
        user.LockedUntilAt = DateTimeOffset.UtcNow.AddMinutes(15);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.LoginAsync("lockeduser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account is locked");
    }

    [Fact]
    public async Task LoginAsync_ShouldIncrementLoginSuccessCount()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("countuser", "password");
        var initialCount = user.LoginSuccessCount;

        // Act
        await _authService.LoginAsync("countuser", "password", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.UserId);
        updatedUser!.LoginSuccessCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldIncrementFailureAttempts()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("failuser", "password");

        // Act
        await _authService.LoginAsync("failuser", "wrongpassword", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.UserId);
        updatedUser!.LoginFailureAttempts.Should().Be(1);
        updatedUser.LoginFailureCount.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_After5FailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("lockableuser", "password");

        // Act - 5回失敗
        for (int i = 0; i < 5; i++)
        {
            await _authService.LoginAsync("lockableuser", "wrongpassword", "TestApp", "127.0.0.1");
        }

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.UserId);
        updatedUser!.LockedUntilAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ShouldResetFailureAttempts()
    {
        // Arrange
        var (user, _) = await SeedTestUserAsync("resetuser", "password");
        user.LoginFailureAttempts = 3;
        await _context.SaveChangesAsync();

        // Act
        await _authService.LoginAsync("resetuser", "password", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.UserId);
        updatedUser!.LoginFailureAttempts.Should().Be(0);
    }

    [Fact]
    public async Task LoginAsync_ShouldCreateSession()
    {
        // Arrange
        await SeedTestUserAsync("sessionuser", "password");

        // Act
        var result = await _authService.LoginAsync("sessionuser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.SessionId.Should().NotBeNull();
        var session = await _context.Sessions.FindAsync(result.SessionId);
        session.Should().NotBeNull();
        session!.ClientAppName.Should().Be("TestApp");
        session.ClientEndpoint.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task LogoutAsync_WithValidSession_ShouldSucceed()
    {
        // Arrange
        await SeedTestUserAsync("logoutuser", "password");
        var loginResult = await _authService.LoginAsync("logoutuser", "password", "TestApp", "127.0.0.1");

        // Act
        var logoutResult = await _authService.LogoutAsync(loginResult.SessionId!.Value);

        // Assert
        logoutResult.Should().BeTrue();
        var session = await _context.Sessions.FindAsync(loginResult.SessionId);
        session!.LogoutAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidSession_ShouldReturnFalse()
    {
        // Act
        var result = await _authService.LogoutAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}

