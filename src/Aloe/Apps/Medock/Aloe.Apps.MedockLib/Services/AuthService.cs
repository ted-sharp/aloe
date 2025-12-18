using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 認証サービス
/// </summary>
public class AuthService : IAuthService
{
    private readonly IDbContextFactory<MedockDbContext> _contextFactory;
    private readonly PasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CookieSettings _cookieSettings;

    public AuthService(
        IDbContextFactory<MedockDbContext> contextFactory,
        PasswordHasher passwordHasher,
        IDateTimeProvider dateTimeProvider,
        IOptions<CookieSettings> cookieSettings)
    {
        this._contextFactory = contextFactory;
        this._passwordHasher = passwordHasher;
        this._dateTimeProvider = dateTimeProvider;
        this._cookieSettings = cookieSettings.Value;
    }

    /// <summary>
    /// ログイン処理を行います。
    /// </summary>
    public async Task<AuthResult> LoginAsync(string userCode, string password, string appName, string ipAddress, string userAgent)
    {
        using var context = this._contextFactory.CreateDbContext();

        var user = await context.Users
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.FacilityUserRoles)
            .ThenInclude(fur => fur.Role)
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.Facility)
            .ThenInclude(f => f.Tenant)
            .FirstOrDefaultAsync(u => u.UserCode == userCode && !u.IsDeleted);

        if (user == null)
        {
            return AuthResult.Failed("Invalid credentials");
        }

        if (user.LockedUntilAt > this._dateTimeProvider.UtcNow)
        {
            return AuthResult.Failed("Account is locked");
        }

        if (!this._passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            user.LoginFailureAttempts++;
            user.LoginFailureCount++;

            if (user.LoginFailureAttempts >= 5)
            {
                user.LockedUntilAt = this._dateTimeProvider.UtcNow.AddMinutes(15);
            }

            await context.SaveChangesAsync();
            return AuthResult.Failed("Invalid credentials");
        }

        user.LoginFailureAttempts = 0;
        user.LoginSuccessCount++;
        user.LastLoginAt = this._dateTimeProvider.UtcNow;

        var defaultFacility = this.DetermineDefaultFacility(context, user);

        var issuedAt = this._dateTimeProvider.UtcNow;
        var expireMinutes = this._cookieSettings.ExpireTimeSpanMinutes ?? 15;
        var expiresAt = issuedAt.AddMinutes(expireMinutes);
        var refreshTokenExpiration = issuedAt.AddDays(7);

        var session = new Session
        {
            SessionId = Guid.CreateVersion7(),
            UserId = user.UserId,
            UserDisplayName = user.UserDisplayName,
            SecurityStamp = Guid.CreateVersion7(),
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            RevokedAt = null,
            IpAddress = ipAddress ?? String.Empty,
            UserAgent = userAgent ?? String.Empty,
            AppName = appName ?? String.Empty
        };
        context.Sessions.Add(session);

        await context.SaveChangesAsync();

        return this.CreateSuccessResult(user, defaultFacility, refreshTokenExpiration, session.SessionId);
    }

    /// <summary>
    /// クッキー認証を更新します。
    /// </summary>
    public async Task<AuthResult> RefreshTokenAsync(Guid userId, Guid? facilityId = null)
    {
        using var context = this._contextFactory.CreateDbContext();

        var user = await this.GetUserWithRelationsAsync(context, userId);
        if (user == null)
        {
            return AuthResult.Failed("User not found");
        }

        Facility? facility = null;
        if (facilityId.HasValue)
        {
            facility = await this.GetFacilityIfAccessibleAsync(context, user, facilityId.Value);
        }
        facility ??= this.DetermineDefaultFacility(context, user);

        var refreshTokenExpiration = this._dateTimeProvider.UtcNow.AddDays(7);

        return this.CreateSuccessResult(user, facility, refreshTokenExpiration, null);
    }

    /// <summary>
    /// 施設を切り替えて新しいトークンを発行します。
    /// </summary>
    public async Task<AuthResult> SwitchFacilityAsync(Guid userId, Guid facilityId)
    {
        using var context = this._contextFactory.CreateDbContext();

        var user = await this.GetUserWithRelationsAsync(context, userId);
        if (user == null)
        {
            return AuthResult.Failed("User not found");
        }

        var facility = await this.GetFacilityIfAccessibleAsync(context, user, facilityId);
        if (facility == null)
        {
            return AuthResult.Failed("Access denied to facility");
        }

        var refreshTokenExpiration = this._dateTimeProvider.UtcNow.AddDays(7);

        return this.CreateSuccessResult(user, facility, refreshTokenExpiration, null);
    }

    /// <summary>
    /// ユーザーがアクセス可能な施設一覧を取得します。
    /// </summary>
    public async Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync(Guid userId)
    {
        using var context = this._contextFactory.CreateDbContext();

        var user = await this.GetUserWithRelationsAsync(context, userId);
        if (user == null)
        {
            return [];
        }

        if (user.IsSystemAdmin)
        {
            var allFacilities = await context.Facilities
                .Include(f => f.Tenant)
                .Where(f => f.IsActive && !f.IsDeleted && f.Tenant.IsActive && !f.Tenant.IsDeleted)
                .OrderBy(f => f.Tenant.TenantName)
                .ThenBy(f => f.FacilityName)
                .ToListAsync();

            return allFacilities.Select(f => new FacilityInfo
            {
                FacilityId = f.FacilityId,
                FacilityName = AuthorizationHelper.GetFacilityDisplayName(f),
                TenantId = f.TenantId,
                TenantName = f.Tenant.TenantName,
            }).OrderBy(f => f.TenantName).ThenBy(f => f.FacilityName).ToList();
        }

        var facilities = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && fu.Facility != null && fu.Facility.IsActive && !fu.Facility.IsDeleted)
            .Select(fu => new FacilityInfo
            {
                FacilityId = fu.FacilityId,
                FacilityName = AuthorizationHelper.GetFacilityDisplayName(fu.Facility),
                TenantId = fu.Facility.TenantId,
                TenantName = fu.Facility.Tenant?.TenantName ?? String.Empty,
            })
            .OrderBy(f => f.TenantName)
            .ThenBy(f => f.FacilityName)
            .ToList();

        return facilities;
    }

    /// <summary>
    /// セッションを検証します。
    /// </summary>
    public async Task<SessionValidationResult> ValidateSessionAsync(Guid sessionId)
    {
        using var context = this._contextFactory.CreateDbContext();

        var session = await context.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            return SessionValidationResult.Invalid("Session not found");
        }

        if (session.RevokedAt.HasValue)
        {
            return SessionValidationResult.Invalid("Session has been revoked");
        }

        if (session.ExpiresAt < this._dateTimeProvider.UtcNow)
        {
            return SessionValidationResult.Invalid("Session has expired");
        }

        var user = await context.Users.FindAsync(session.UserId);
        if (user == null || user.IsDeleted)
        {
            return SessionValidationResult.Invalid("User not found or deleted");
        }

        if (user.LockedUntilAt > this._dateTimeProvider.UtcNow)
        {
            return SessionValidationResult.Invalid("Account is locked");
        }

        return SessionValidationResult.Valid();
    }

    /// <summary>
    /// ログアウト処理を行います。
    /// </summary>
    public async Task<bool> LogoutAsync(Guid sessionId)
    {
        using var context = this._contextFactory.CreateDbContext();

        var session = await context.Sessions.FindAsync(sessionId);
        if (session == null || session.RevokedAt.HasValue)
        {
            return false;
        }

        session.RevokedAt = this._dateTimeProvider.UtcNow;

        var user = await context.Users.FindAsync(session.UserId);
        if (user != null)
        {
            user.LastLogoutAt = this._dateTimeProvider.UtcNow;
        }

        await context.SaveChangesAsync();
        return true;
    }

    private AuthResult CreateSuccessResult(User user, Facility? facility, DateTime refreshTokenExpiration, Guid? sessionId)
    {
        var roles = AuthorizationHelper.GetUserRoles(user, facility?.FacilityId);
        var isFacilityAdmin = facility != null && AuthorizationHelper.IsFacilityAdmin(user, facility.FacilityId);

        return AuthResult.Success(
            refreshTokenExpiration,
            sessionId,
            user.UserId,
            user.UserCode,
            user.Email,
            user.UserDisplayName,
            facility?.TenantId,
            facility?.Tenant?.TenantName,
            facility?.FacilityId,
            AuthorizationHelper.GetFacilityDisplayName(facility),
            user.IsSystemAdmin,
            isFacilityAdmin,
            roles);
    }

    private async Task<User?> GetUserWithRelationsAsync(MedockDbContext context, Guid userId)
    {
        return await context.Users
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.FacilityUserRoles)
            .ThenInclude(fur => fur.Role)
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.Facility)
            .ThenInclude(f => f.Tenant)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    private Facility? DetermineDefaultFacility(MedockDbContext context, User user)
    {
        var facilityUser = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && fu.Facility != null && fu.Facility.IsActive && !fu.Facility.IsDeleted)
            .OrderBy(fu => fu.FacilityUserSeq)
            .FirstOrDefault();

        if (facilityUser?.Facility != null)
        {
            return facilityUser.Facility;
        }

        if (user.IsSystemAdmin)
        {
            return context.Facilities
                .Include(f => f.Tenant)
                .Where(f => f.IsActive && !f.IsDeleted && f.Tenant.IsActive && !f.Tenant.IsDeleted)
                .FirstOrDefault();
        }

        return null;
    }

    private async Task<Facility?> GetFacilityIfAccessibleAsync(MedockDbContext context, User user, Guid facilityId)
    {
        var facility = await context.Facilities
            .Include(f => f.Tenant)
            .FirstOrDefaultAsync(f => f.FacilityId == facilityId && f.IsActive && !f.IsDeleted);

        if (facility == null) return null;

        if (user.IsSystemAdmin) return facility;

        var hasFacilityAccess = user.FacilityUsers
            .Any(fu => fu.FacilityId == facilityId && !fu.IsDeleted);
        if (hasFacilityAccess) return facility;

        return null;
    }
}
