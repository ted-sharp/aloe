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
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// ログイン処理を行います。
    /// </summary>
    public async Task<AuthResult> LoginAsync(string userCode, string password, string clientAppName, string clientEndpoint)
    {
        // ユーザーを検索
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.TenantUsers)
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
        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            // 失敗回数を増加
            user.LoginFailureAttempts++;
            user.LoginFailureCount++;

            // 一定回数失敗でロック
            if (user.LoginFailureAttempts >= 5)
            {
                user.LockedUntilAt = DateTimeOffset.UtcNow.AddMinutes(15);
            }

            await _context.SaveChangesAsync();
            return AuthResult.Failed("Invalid credentials");
        }

        // ログイン成功
        user.LoginFailureAttempts = 0;
        user.LoginSuccessCount++;
        user.LastLoginAt = DateTimeOffset.UtcNow;

        // セッション作成
        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            UserDisplayName = user.TenantUsers.FirstOrDefault()?.DisplayName ?? user.UserCode,
            ClientAppName = clientAppName,
            ClientEndpoint = clientEndpoint,
            LoginAt = DateTimeOffset.UtcNow
        };
        _context.Sessions.Add(session);

        await _context.SaveChangesAsync();

        // ロール一覧を取得
        var roles = user.UserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.RoleCode)
            .ToArray();

        // テナントID取得（単一テナントの場合）
        var tenantUser = user.TenantUsers.FirstOrDefault(tu => !tu.IsDeleted);
        var tenantId = tenantUser?.TenantId;

        // トークン生成
        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.UserId,
            user.UserCode,
            user.Email,
            tenantId,
            roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = _jwtTokenService.GetRefreshTokenExpiration();

        return AuthResult.Success(
            accessToken,
            refreshToken,
            refreshTokenExpiration,
            session.SessionId,
            user.UserId,
            user.UserCode,
            user.Email,
            tenantId,
            roles);
    }

    /// <summary>
    /// リフレッシュトークンを使用してアクセストークンを更新します。
    /// </summary>
    public async Task<AuthResult> RefreshTokenAsync(Guid userId, string refreshToken)
    {
        // TODO: リフレッシュトークンの検証ロジックを実装
        // 現在は簡易実装として、ユーザーが存在すれば新しいトークンを発行
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Include(u => u.TenantUsers)
            .FirstOrDefaultAsync(u => u.UserId == userId && !u.IsDeleted);

        if (user == null)
        {
            return AuthResult.Failed("Invalid refresh token");
        }

        var roles = user.UserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => ur.RoleCode)
            .ToArray();

        var tenantUser = user.TenantUsers.FirstOrDefault(tu => !tu.IsDeleted);
        var tenantId = tenantUser?.TenantId;

        var accessToken = _jwtTokenService.GenerateAccessToken(
            user.UserId,
            user.UserCode,
            user.Email,
            tenantId,
            roles);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiration = _jwtTokenService.GetRefreshTokenExpiration();

        return AuthResult.Success(
            accessToken,
            newRefreshToken,
            refreshTokenExpiration,
            null, // セッションIDは更新しない
            user.UserId,
            user.UserCode,
            user.Email,
            tenantId,
            roles);
    }

    /// <summary>
    /// ログアウト処理を行います。
    /// </summary>
    public async Task<bool> LogoutAsync(Guid sessionId)
    {
        var session = await _context.Sessions.FindAsync(sessionId);
        if (session == null || session.LogoutAt.HasValue)
        {
            return false;
        }

        session.LogoutAt = DateTimeOffset.UtcNow;

        var user = await _context.Users.FindAsync(session.UserId);
        if (user != null)
        {
            user.LastLogoutAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
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
    public Guid? TenantId { get; init; }
    public string[]? Roles { get; init; }

    public static AuthResult Success(
        string accessToken,
        string refreshToken,
        DateTime refreshTokenExpiration,
        Guid? sessionId,
        Guid userId,
        string userCode,
        string email,
        Guid? tenantId,
        string[] roles)
    {
        return new AuthResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiration = refreshTokenExpiration,
            SessionId = sessionId,
            UserId = userId,
            UserCode = userCode,
            Email = email,
            TenantId = tenantId,
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


