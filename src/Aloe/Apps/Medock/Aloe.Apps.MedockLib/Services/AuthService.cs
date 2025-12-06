using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 認証サービス
/// </summary>
public class AuthService
{
    private readonly MedockDbContext _context;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        MedockDbContext context,
        PasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        this._context = context;
        this._passwordHasher = passwordHasher;
        this._jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// ログイン処理を行います。
    /// </summary>
    public async Task<AuthResult> LoginAsync(string userCode, string password, string clientAppName, string clientEndpoint)
    {
        // ユーザーを検索（テナント・施設情報も含む）
        var user = await this._context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.Facility)
            .ThenInclude(f => f.Tenant)
            .FirstOrDefaultAsync(u => u.UserCode == userCode && !u.IsDeleted);

        if (user == null)
        {
            return AuthResult.Failed("Invalid credentials");
        }

        // アカウントロック確認
        if (user.LockedUntilAt > DateTimeOffset.UtcNow)
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
                user.LockedUntilAt = DateTimeOffset.UtcNow.AddMinutes(15);
            }

            await this._context.SaveChangesAsync();
            return AuthResult.Failed("Invalid credentials");
        }

        // ログイン成功
        user.LoginFailureAttempts = 0;
        user.LoginSuccessCount++;
        user.LastLoginAt = DateTimeOffset.UtcNow;

        // デフォルト施設を決定
        var defaultFacility = this.DetermineDefaultFacility(user);

        // セッション作成
        var displayName = this.GetDisplayName(user, defaultFacility?.FacilityId);
        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            UserDisplayName = displayName,
            ClientAppName = clientAppName,
            ClientEndpoint = clientEndpoint,
            LoginAt = DateTimeOffset.UtcNow
        };
        this._context.Sessions.Add(session);

        await this._context.SaveChangesAsync();

        // トークン生成パラメータ作成
        var tokenParams = this.CreateTokenParams(user, defaultFacility);
        var accessToken = this._jwtTokenService.GenerateAccessToken(tokenParams);
        var refreshToken = this._jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = this._jwtTokenService.GetRefreshTokenExpiration();

        return AuthResult.Success(
            accessToken,
            refreshToken,
            refreshTokenExpiration,
            session.SessionId,
            tokenParams);
    }

    /// <summary>
    /// リフレッシュトークンを使用してアクセストークンを更新します。
    /// </summary>
    public async Task<AuthResult> RefreshTokenAsync(Guid userId, string refreshToken, Guid? facilityId = null)
    {
        // TODO: リフレッシュトークンの検証ロジックを実装
        var user = await this.GetUserWithRelationsAsync(userId);

        if (user == null)
        {
            return AuthResult.Failed("Invalid refresh token");
        }

        // 施設IDが指定されていれば使用、なければデフォルト
        Facility? facility = null;
        if (facilityId.HasValue)
        {
            facility = await this.GetFacilityIfAccessibleAsync(user, facilityId.Value);
        }
        facility ??= this.DetermineDefaultFacility(user);

        var tokenParams = this.CreateTokenParams(user, facility);
        var accessToken = this._jwtTokenService.GenerateAccessToken(tokenParams);
        var newRefreshToken = this._jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = this._jwtTokenService.GetRefreshTokenExpiration();

        return AuthResult.Success(
            accessToken,
            newRefreshToken,
            refreshTokenExpiration,
            null,
            tokenParams);
    }

    /// <summary>
    /// 施設を切り替えて新しいトークンを発行します。
    /// </summary>
    public async Task<AuthResult> SwitchFacilityAsync(Guid userId, Guid facilityId)
    {
        var user = await this.GetUserWithRelationsAsync(userId);
        if (user == null)
        {
            return AuthResult.Failed("User not found");
        }

        // 施設へのアクセス権限チェック
        var facility = await this.GetFacilityIfAccessibleAsync(user, facilityId);
        if (facility == null)
        {
            return AuthResult.Failed("Access denied to facility");
        }

        var tokenParams = this.CreateTokenParams(user, facility);
        var accessToken = this._jwtTokenService.GenerateAccessToken(tokenParams);
        var refreshToken = this._jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = this._jwtTokenService.GetRefreshTokenExpiration();

        return AuthResult.Success(
            accessToken,
            refreshToken,
            refreshTokenExpiration,
            null,
            tokenParams);
    }

    /// <summary>
    /// ユーザーがアクセス可能な施設一覧を取得します。
    /// </summary>
    public async Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync(Guid userId)
    {
        var user = await this.GetUserWithRelationsAsync(userId);
        if (user == null)
        {
            return new List<FacilityInfo>();
        }

        // システム管理者: 全施設
        if (user.IsSystemAdmin)
        {
            var allFacilities = await this._context.Facilities
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
                PermissionLevel = "full"
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
                PermissionLevel = fu.PermissionLevel ?? "full"
            })
            .OrderBy(f => f.TenantName)
            .ThenBy(f => f.FacilityName)
            .ToList();

        return facilities;
    }

    /// <summary>
    /// ログアウト処理を行います。
    /// </summary>
    public async Task<bool> LogoutAsync(Guid sessionId)
    {
        var session = await this._context.Sessions.FindAsync(sessionId);
        if (session == null || session.LogoutAt.HasValue)
        {
            return false;
        }

        session.LogoutAt = DateTimeOffset.UtcNow;

        var user = await this._context.Users.FindAsync(session.UserId);
        if (user != null)
        {
            user.LastLogoutAt = DateTimeOffset.UtcNow;
        }

        await this._context.SaveChangesAsync();
        return true;
    }

    private async Task<User?> GetUserWithRelationsAsync(Guid userId)
    {
        return await this._context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.FacilityUsers)
            .ThenInclude(fu => fu.Facility)
            .ThenInclude(f => f.Tenant)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);
    }

    private Facility? DetermineDefaultFacility(User user)
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
            return this._context.Facilities
                .Include(f => f.Tenant)
                .Where(f => f.IsActive && !f.IsDeleted && f.Tenant.IsActive && !f.Tenant.IsDeleted)
                .FirstOrDefault();
        }

        return null;
    }

    private async Task<Facility?> GetFacilityIfAccessibleAsync(User user, Guid facilityId)
    {
        var facility = await this._context.Facilities
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

    private string GetDisplayName(User user, Guid? facilityId)
    {
        // 施設ユーザーの表示名（指定施設がある場合）
        if (facilityId.HasValue)
        {
            var facilityUser = user.FacilityUsers
                .FirstOrDefault(fu => fu.FacilityId == facilityId && !fu.IsDeleted);
            if (facilityUser != null && !String.IsNullOrEmpty(facilityUser.DisplayName))
            {
                return facilityUser.DisplayName;
            }
        }

        // Fallback: 最初の施設ユーザーの表示名
        var anyFacilityUser = user.FacilityUsers
            .Where(fu => !fu.IsDeleted)
            .OrderBy(fu => fu.FacilityUserSeq)
            .FirstOrDefault();
        if (anyFacilityUser != null && !String.IsNullOrEmpty(anyFacilityUser.DisplayName))
        {
            return anyFacilityUser.DisplayName;
        }

        return user.UserCode;
    }

    private TokenGenerationParams CreateTokenParams(User user, Facility? facility)
    {
        var roles = user.UserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.RoleCode)
            .ToArray();

        // 施設管理者かチェック
        var isFacilityAdmin = facility != null && user.FacilityUsers
            .Any(fu => fu.FacilityId == facility.FacilityId && fu.IsFacilityAdmin && !fu.IsDeleted);

        return new TokenGenerationParams
        {
            UserId = user.UserId,
            UserCode = user.UserCode,
            Email = user.Email,
            DisplayName = this.GetDisplayName(user, facility?.FacilityId),
            TenantId = facility?.TenantId,
            TenantName = facility?.Tenant?.TenantName,
            FacilityId = facility?.FacilityId,
            FacilityName = facility != null
                ? (!String.IsNullOrEmpty(facility.FacilityNameDisplay) ? facility.FacilityNameDisplay : facility.FacilityName)
                : null,
            IsSystemAdmin = user.IsSystemAdmin,
            IsFacilityAdmin = isFacilityAdmin,
            Roles = roles
        };
    }
}

/// <summary>
/// 認証結果
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
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
        string accessToken,
        string refreshToken,
        DateTime refreshTokenExpiration,
        Guid? sessionId,
        TokenGenerationParams tokenParams)
    {
        return new AuthResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
            SessionId = sessionId,
            UserId = tokenParams.UserId,
            UserCode = tokenParams.UserCode,
            Email = tokenParams.Email,
            DisplayName = tokenParams.DisplayName,
            TenantId = tokenParams.TenantId,
            TenantName = tokenParams.TenantName,
            FacilityId = tokenParams.FacilityId,
            FacilityName = tokenParams.FacilityName,
            IsSystemAdmin = tokenParams.IsSystemAdmin,
            IsFacilityAdmin = tokenParams.IsFacilityAdmin,
            Roles = tokenParams.Roles?.ToArray()
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
    public string PermissionLevel { get; init; } = "";
}
