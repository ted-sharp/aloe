using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
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

        // ユーザーを検索（テナント・施設情報も含む）
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

        // アカウントロック確認
        if (user.LockedUntilAt > this._dateTimeProvider.UtcNow)
        {
            return AuthResult.Failed("Account is locked");
        }

        // パスワード検証
        if (!this._passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            // 失敗回数を増加
            user.LoginFailureAttempts++;
            user.LoginFailureCount++;

            // 一定回数失敗でロック
            if (user.LoginFailureAttempts >= 5)
            {
                user.LockedUntilAt = this._dateTimeProvider.UtcNow.AddMinutes(15);
            }

            await context.SaveChangesAsync();
            return AuthResult.Failed("Invalid credentials");
        }

        // ログイン成功
        user.LoginFailureAttempts = 0;
        user.LoginSuccessCount++;
        user.LastLoginAt = this._dateTimeProvider.UtcNow;

        // デフォルト施設を決定
        var defaultFacility = this.DetermineDefaultFacility(context, user);

        // セッション作成
        var issuedAt = this._dateTimeProvider.UtcNow;
        var expireMinutes = this._cookieSettings.ExpireTimeSpanMinutes ?? 15;
        var expiresAt = issuedAt.AddMinutes(expireMinutes);
        var refreshTokenExpiration = issuedAt.AddDays(7); // デフォルト7日間

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            UserDisplayName = user.UserDisplayName,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            RevokedAt = null,
            IpAddress = ipAddress ?? String.Empty,
            UserAgent = userAgent ?? String.Empty,
            AppName = appName ?? String.Empty
        };
        context.Sessions.Add(session);

        await context.SaveChangesAsync();

        // 施設に関連するロールを取得
        var roles = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && (defaultFacility == null || fu.FacilityId == defaultFacility.FacilityId))
            .SelectMany(fu => fu.FacilityUserRoles)
            .Where(fur => !fur.IsDeleted)
            .Select(fur => fur.RoleCode)
            .Distinct()
            .ToArray();

        // 施設管理者かチェック
        var isFacilityAdmin = defaultFacility != null && user.FacilityUsers
            .Any(fu => fu.FacilityId == defaultFacility.FacilityId && fu.IsFacilityAdmin && !fu.IsDeleted);

        return AuthResult.Success(
            refreshTokenExpiration,
            session.SessionId,
            user.UserId,
            user.UserCode,
            user.Email,
            user.UserDisplayName,
            defaultFacility?.TenantId,
            defaultFacility?.Tenant?.TenantName,
            defaultFacility?.FacilityId,
            defaultFacility != null
                ? (!String.IsNullOrEmpty(defaultFacility.FacilityNameDisplay) ? defaultFacility.FacilityNameDisplay : defaultFacility.FacilityName)
                : null,
            user.IsSystemAdmin,
            isFacilityAdmin,
            roles);
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

        // 施設IDが指定されていれば使用、なければデフォルト
        Facility? facility = null;
        if (facilityId.HasValue)
        {
            facility = await this.GetFacilityIfAccessibleAsync(context, user, facilityId.Value);
        }
        facility ??= this.DetermineDefaultFacility(context, user);

        // 施設に関連するロールを取得
        var roles = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && (facility == null || fu.FacilityId == facility.FacilityId))
            .SelectMany(fu => fu.FacilityUserRoles)
            .Where(fur => !fur.IsDeleted)
            .Select(fur => fur.RoleCode)
            .Distinct()
            .ToArray();

        // 施設管理者かチェック
        var isFacilityAdmin = facility != null && user.FacilityUsers
            .Any(fu => fu.FacilityId == facility.FacilityId && fu.IsFacilityAdmin && !fu.IsDeleted);

        var refreshTokenExpiration = this._dateTimeProvider.UtcNow.AddDays(7); // デフォルト7日間

        return AuthResult.Success(
            refreshTokenExpiration,
            null,
            user.UserId,
            user.UserCode,
            user.Email,
            user.UserDisplayName,
            facility?.TenantId,
            facility?.Tenant?.TenantName,
            facility?.FacilityId,
            facility != null
                ? (!String.IsNullOrEmpty(facility.FacilityNameDisplay) ? facility.FacilityNameDisplay : facility.FacilityName)
                : null,
            user.IsSystemAdmin,
            isFacilityAdmin,
            roles);
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

        // 施設へのアクセス権限チェック
        var facility = await this.GetFacilityIfAccessibleAsync(context, user, facilityId);
        if (facility == null)
        {
            return AuthResult.Failed("Access denied to facility");
        }

        // 施設に関連するロールを取得
        var roles = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && fu.FacilityId == facility.FacilityId)
            .SelectMany(fu => fu.FacilityUserRoles)
            .Where(fur => !fur.IsDeleted)
            .Select(fur => fur.RoleCode)
            .Distinct()
            .ToArray();

        // 施設管理者かチェック
        var isFacilityAdmin = user.FacilityUsers
            .Any(fu => fu.FacilityId == facility.FacilityId && fu.IsFacilityAdmin && !fu.IsDeleted);

        var refreshTokenExpiration = this._dateTimeProvider.UtcNow.AddDays(7); // デフォルト7日間

        return AuthResult.Success(
            refreshTokenExpiration,
            null,
            user.UserId,
            user.UserCode,
            user.Email,
            user.UserDisplayName,
            facility.TenantId,
            facility.Tenant?.TenantName,
            facility.FacilityId,
            !String.IsNullOrEmpty(facility.FacilityNameDisplay) ? facility.FacilityNameDisplay : facility.FacilityName,
            user.IsSystemAdmin,
            isFacilityAdmin,
            roles);
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

        // システム管理者: 全施設
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
                FacilityName = !String.IsNullOrEmpty(f.FacilityNameDisplay) ? f.FacilityNameDisplay : f.FacilityName,
                TenantId = f.TenantId,
                TenantName = f.Tenant.TenantName,
            }).OrderBy(f => f.TenantName).ThenBy(f => f.FacilityName).ToList();
        }

        // 一般ユーザー: FacilityUser で明示的に割り当てられた施設のみ
        var facilities = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && fu.Facility != null && fu.Facility.IsActive && !fu.Facility.IsDeleted)
            .Select(fu => new FacilityInfo
            {
                FacilityId = fu.FacilityId,
                FacilityName = !String.IsNullOrEmpty(fu.Facility.FacilityNameDisplay)
                    ? fu.Facility.FacilityNameDisplay
                    : fu.Facility.FacilityName,
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
    /// <param name="sessionId">セッションID</param>
    /// <returns>検証結果</returns>
    public async Task<SessionValidationResult> ValidateSessionAsync(Guid sessionId)
    {
        using var context = this._contextFactory.CreateDbContext();

        // セッションの存在確認
        var session = await context.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            return SessionValidationResult.Invalid("Session not found");
        }

        // セッションが無効化済みかチェック
        if (session.RevokedAt.HasValue)
        {
            return SessionValidationResult.Invalid("Session has been revoked");
        }

        // セッションの有効期限チェック
        if (session.ExpiresAt < this._dateTimeProvider.UtcNow)
        {
            return SessionValidationResult.Invalid("Session has expired");
        }

        // ユーザーの存在確認
        var user = await context.Users.FindAsync(session.UserId);
        if (user == null || user.IsDeleted)
        {
            return SessionValidationResult.Invalid("User not found or deleted");
        }

        // アカウントロック確認
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
        // 1. 施設ユーザーとして登録されている最初の施設（sequence順）
        var facilityUser = user.FacilityUsers
            .Where(fu => !fu.IsDeleted && fu.Facility != null && fu.Facility.IsActive && !fu.Facility.IsDeleted)
            .OrderBy(fu => fu.FacilityUserSeq)
            .FirstOrDefault();

        if (facilityUser?.Facility != null)
        {
            return facilityUser.Facility;
        }

        // 2. システム管理者の場合は最初の有効な施設
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

        // システム管理者は全施設アクセス可
        if (user.IsSystemAdmin) return facility;

        // 施設ユーザーチェック
        var hasFacilityAccess = user.FacilityUsers
            .Any(fu => fu.FacilityId == facilityId && !fu.IsDeleted);
        if (hasFacilityAccess) return facility;

        return null;
    }
}

/// <summary>
/// 認証結果
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? RefreshTokenExpiration { get; init; }
    public Guid? SessionId { get; init; }
    public Guid? UserId { get; init; }
    public string? UserCode { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
    public Guid? FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public bool IsSystemAdmin { get; init; }
    public bool IsFacilityAdmin { get; init; }
    public string[]? Roles { get; init; }

    public static AuthResult Success(
        DateTime refreshTokenExpiration,
        Guid? sessionId,
        Guid userId,
        string userCode,
        string email,
        string userDisplayName,
        Guid? tenantId,
        string? tenantName,
        Guid? facilityId,
        string? facilityName,
        bool isSystemAdmin,
        bool isFacilityAdmin,
        string[] roles)
    {
        return new AuthResult
        {
            IsSuccess = true,
            RefreshTokenExpiration = refreshTokenExpiration,
            SessionId = sessionId,
            UserId = userId,
            UserCode = userCode,
            Email = email,
            DisplayName = userDisplayName,
            TenantId = tenantId,
            TenantName = tenantName ?? "",
            FacilityId = facilityId,
            FacilityName = facilityName ?? "",
            IsSystemAdmin = isSystemAdmin,
            IsFacilityAdmin = isFacilityAdmin,
            Roles = roles
        };
    }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// 施設情報
/// </summary>
public class FacilityInfo
{
    public Guid FacilityId { get; init; }
    public string FacilityName { get; init; } = "";
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = "";
    public bool IsFacilityAdmin { get; init; }
}

/// <summary>
/// セッション検証結果
/// </summary>
public class SessionValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    private SessionValidationResult(bool isValid, string? errorMessage = null)
    {
        this.IsValid = isValid;
        this.ErrorMessage = errorMessage;
    }

    public static SessionValidationResult Valid()
    {
        return new SessionValidationResult(true);
    }

    public static SessionValidationResult Invalid(string errorMessage)
    {
        return new SessionValidationResult(false, errorMessage);
    }
}
