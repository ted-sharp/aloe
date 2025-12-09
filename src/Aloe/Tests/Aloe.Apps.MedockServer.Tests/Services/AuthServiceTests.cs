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
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly PasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        // InMemory Database
        var options = new DbContextOptionsBuilder<MedockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        this._context = new MedockDbContext(options);

        // DbContextFactory (テスト用のシンプルな実装)
        this._contextFactory = new TestDbContextFactory(this._context);

        // PasswordHasher
        this._passwordHasher = PasswordHasher.Default;

        // DateTimeProvider
        this._dateTimeProvider = new JstDateTimeProvider();

        // JwtTokenService
        var jwtSettings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };
        this._jwtTokenService = new JwtTokenService(Options.Create(jwtSettings), this._dateTimeProvider);

        // AuthService
        this._authService = new AuthService(this._contextFactory, this._passwordHasher, this._jwtTokenService, this._dateTimeProvider);
    }

    public void Dispose()
    {
        this._context.Dispose();
    }

    private async Task<(User user, Facility facility)> SeedTestUserAsync(string userCode = "testuser", string password = "testpass")
    {
        var (hash, salt) = this._passwordHasher.HashPassword(password);

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            TenantName = "テストテナント",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = this._dateTimeProvider.UtcNow,
            UpdatedAt = this._dateTimeProvider.UtcNow
        };
        this._context.Tenants.Add(tenant);

        var facility = new Facility
        {
            FacilityId = Guid.NewGuid(),
            TenantId = tenant.TenantId,
            MedicalInstitutionCode = "1234567890",
            FacilityName = "テスト施設",
            FacilityNameDisplay = "テスト施設",
            IsActive = true,
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt = this._dateTimeProvider.UtcNow,
            UpdatedAt = this._dateTimeProvider.UtcNow
        };
        this._context.Facilities.Add(facility);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            UserCode = userCode,
            Email = $"{userCode}@example.com",
            PasswordHash = hash,
            PasswordSalt = salt,
            IsSystemAdmin = false,
            IsDeleted = false,
            CreatedAt = this._dateTimeProvider.UtcNow,
            UpdatedAt = this._dateTimeProvider.UtcNow
        };
        this._context.Users.Add(user);

        var facilityUser = new FacilityUser
        {
            FacilityUserId = Guid.NewGuid(),
            FacilityId = facility.FacilityId,
            UserId = user.UserId,
            DisplayName = "テストユーザー",
            FacilityUserSeq = 1,
            IsFacilityAdmin = true,
            CreatedAt = this._dateTimeProvider.UtcNow,
            UpdatedAt = this._dateTimeProvider.UtcNow
        };
        this._context.FacilityUsers.Add(facilityUser);

        await this._context.SaveChangesAsync();

        return (user, facility);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        var (user, facility) = await this.SeedTestUserAsync("admin", "admin");

        // Act
        var result = await this._authService.LoginAsync("admin", "admin", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.UserId.Should().Be(user.UserId);
        result.UserCode.Should().Be("admin");
        result.FacilityId.Should().Be(facility.FacilityId);
        result.SessionId.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUserCode_ShouldFail()
    {
        // Arrange
        await this.SeedTestUserAsync("admin", "admin");

        // Act
        var result = await this._authService.LoginAsync("wronguser", "admin", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.AccessToken.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldFail()
    {
        // Arrange
        await this.SeedTestUserAsync("admin", "admin");

        // Act
        var result = await this._authService.LoginAsync("admin", "wrongpassword", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
        result.AccessToken.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ShouldFail()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("deleteduser", "password");
        user.IsDeleted = true;
        await this._context.SaveChangesAsync();

        // Act
        var result = await this._authService.LoginAsync("deleteduser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid credentials");
    }

    [Fact]
    public async Task LoginAsync_WithLockedUser_ShouldFail()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("lockeduser", "password");
        user.LockedUntilAt = this._dateTimeProvider.UtcNow.AddMinutes(15);
        await this._context.SaveChangesAsync();

        // Act
        var result = await this._authService.LoginAsync("lockeduser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account is locked");
    }

    [Fact]
    public async Task LoginAsync_ShouldIncrementLoginSuccessCount()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("countuser", "password");
        var initialCount = user.LoginSuccessCount;

        // Act
        await this._authService.LoginAsync("countuser", "password", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await this._context.Users.FindAsync(user.UserId);
        updatedUser!.LoginSuccessCount.Should().Be(initialCount + 1);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldIncrementFailureAttempts()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("failuser", "password");

        // Act
        await this._authService.LoginAsync("failuser", "wrongpassword", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await this._context.Users.FindAsync(user.UserId);
        updatedUser!.LoginFailureAttempts.Should().Be(1);
        updatedUser.LoginFailureCount.Should().Be(1);
    }

    [Fact]
    public async Task LoginAsync_After5FailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("lockableuser", "password");

        // Act - 5回失敗
        for (int i = 0; i < 5; i++)
        {
            await this._authService.LoginAsync("lockableuser", "wrongpassword", "TestApp", "127.0.0.1");
        }

        // Assert
        var updatedUser = await this._context.Users.FindAsync(user.UserId);
        updatedUser!.LockedUntilAt.Should().BeAfter(this._dateTimeProvider.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulLogin_ShouldResetFailureAttempts()
    {
        // Arrange
        var (user, _) = await this.SeedTestUserAsync("resetuser", "password");
        user.LoginFailureAttempts = 3;
        await this._context.SaveChangesAsync();

        // Act
        await this._authService.LoginAsync("resetuser", "password", "TestApp", "127.0.0.1");

        // Assert
        var updatedUser = await this._context.Users.FindAsync(user.UserId);
        updatedUser!.LoginFailureAttempts.Should().Be(0);
    }

    [Fact]
    public async Task LoginAsync_ShouldCreateSession()
    {
        // Arrange
        await this.SeedTestUserAsync("sessionuser", "password");

        // Act
        var result = await this._authService.LoginAsync("sessionuser", "password", "TestApp", "127.0.0.1");

        // Assert
        result.SessionId.Should().NotBeNull();
        var session = await this._context.Sessions.FindAsync(result.SessionId);
        session.Should().NotBeNull();
        session!.ClientAppName.Should().Be("TestApp");
        session.ClientEndpoint.Should().Be("127.0.0.1");
    }

    [Fact]
    public async Task LogoutAsync_WithValidSession_ShouldSucceed()
    {
        // Arrange
        await this.SeedTestUserAsync("logoutuser", "password");
        var loginResult = await this._authService.LoginAsync("logoutuser", "password", "TestApp", "127.0.0.1");

        // Act
        var logoutResult = await this._authService.LogoutAsync(loginResult.SessionId!.Value);

        // Assert
        logoutResult.Should().BeTrue();
        var session = await this._context.Sessions.FindAsync(loginResult.SessionId);
        session!.LogoutAt.Should().NotBeNull();
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidSession_ShouldReturnFalse()
    {
        // Act
        var result = await this._authService.LogoutAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// テスト用のDbContextFactory実装
    /// </summary>
    private class TestDbContextFactory : IDbContextFactory<MedockDbContext>
    {
        private readonly MedockDbContext _context;

        public TestDbContextFactory(MedockDbContext context)
        {
            this._context = context;
        }

        public MedockDbContext CreateDbContext()
        {
            return this._context;
        }
    }
}

