using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

/// <summary>
/// ログイン, ログアウト用のサービスです。
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// データベース接続文字列の Host/Server の記述を取得します。
    /// </summary>
    Task<string> GetDbHostAsync();

    /// <summary>
    /// データベース接続文字列の Database の記述を取得します。
    /// </summary>
    Task<string> GetDbNameAsync();

    /// <summary>
    /// DB接続とデータアクセスを試行します。
    /// EFCore は初回アクセス時にマッピングなどが行われるため時間がかかります。
    /// </summary>
    Task PreloadAsync();

    // TODO: バージョンアップで増える可能性のある項目は、どこかで自動的にDBに入れたい。
    // ポリシー、プリファレンス、パーミッションあたりか？

    /// <summary>
    /// ログインを試行します。
    /// </summary>
    Task<LoginResult> LoginAsync(LoginRequest request);

    /// <summary>
    /// ログアウトします。
    /// </summary>
    Task LogoutAsync(SessionDto session);
}

public class AuthService : IAuthService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IPolicyService _policyService;

    public AuthService(
        ILogger<AuthService> logger,
        IDbContextFactory<AppDbContext> factory,
        IPolicyService policyService)
    {
        this._logger = logger;
        this._factory = factory;
        this._policyService = policyService;
    }

    public async Task<string> GetDbHostAsync()
    {
        try
        {
            await using var context = await this._factory.CreateDbContextAsync();
            return context.GetHost();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }

        return "";
    }

    public async Task<string> GetDbNameAsync()
    {
        try
        {
            await using var context = await this._factory.CreateDbContextAsync();
            return context.Database.GetDbConnection().Database;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }

        return "";
    }

    public async Task PreloadAsync()
    {
        try
        {
            await using var context = await this._factory.CreateDbContextAsync();
            _ = await context.Policies.AsNoTracking().FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var result = new LoginResult
        {
            IsSuccess = false,
            IsPasswordInvalid = false,
            ErrorMessage = null,
            SessionDto = null,
            UserDto = null,
        };

        try
        {
            var now = DateTime.Now;

            await using var context = await this._factory.CreateDbContextAsync();
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);
            if (user == null)
            {
                result.ErrorMessage = "ログイン名が正しくありません。";
                return result;
            }

            if (user.IsLocked(now))
            {
                var span = user.LockedUntilAt - now;
                var format = span.ToApproximateJaString();
                result.ErrorMessage = $"ロックされています。(解除まで {format})";
                return result;
            }

            var isCorrect = user.VerifyPassword(request.Password);
            if (!isCorrect)
            {
                result.ErrorMessage = "パスワードが正しくありません。";
                result.IsPasswordInvalid = true;

                var lockingFailAttempts = await this._policyService.GetOrFetchValueAsync<int>(PolicyCode.LoginLockingFailAttempts);
                var lockingSeconds = await this._policyService.GetOrFetchValueAsync<int>(PolicyCode.LoginLockingSeconds);

                user.FailLogin(
                    lockingFailAttempts,
                    lockingSeconds,
                    DateTime.Now);

                // ユーザーのログイン失敗を記録する。
                await context.SaveChangesAsync();
                return result;
            }

            if (user.IsExpired(now.Date))
            {
                result.ErrorMessage = "有効期限を過ぎています。";
                return result;
            }

            var session = await this.CreateAndAddNewSessionAsync(
                context, user, request.ClientAppName, request.ClientEndpoint, now);

            var sessionDto = session.ToSessionDto();
            user.SucceedLogin(sessionDto.LoginAt);
            user.SetUpdatedSession(sessionDto, now);

            // ユーザーのログインとセッション情報を記録する。
            await context.SaveChangesAsync();

            var userDto = user.ToUserDto();
            result.IsSuccess = true;
            result.SessionDto = sessionDto;
            result.UserDto = userDto;
        }
        catch (Exception ex)
        {
            var msg = "ログインで例外が発生しました。";
            this._logger.LogError(ex, msg);

            result.IsSuccess = false;
            result.ErrorMessage = msg;
            result.SessionDto = null;
            result.UserDto = null;
        }

        return result;
    }

    #region Session

    private async Task<Session> CreateAndAddNewSessionAsync(
        AppDbContext context,
        User user,
        string clientAppName,
        string clientEndpoint,
        DateTime now)
    {
        // TODO: セッションを作る
        var newSession = new Session
        {
            // TODO: Guid の生成は PostgreSQL側にした方がかぶるリスクがないはず
            SessionId = Guid.CreateVersion7(),
            UserId = user.UserId,
            UserDisplayName = user.DisplayName,
            ClientAppName = clientAppName,
            ClientEndpoint = clientEndpoint,
            LoginAt = now,
            LogoutAt = null,
        };

        var entity = await context.Sessions.AddAsync(newSession);
        return entity.Entity;
    }

    #endregion Session

    public async Task LogoutAsync(SessionDto sessionDto)
    {
        try
        {
            var now = DateTime.Now;

            await using var context = await this._factory.CreateDbContextAsync();
            var user = await context.Users.FindAsync(sessionDto.UserId);
            if (user != null)
            {
                user.SetUpdatedSession(sessionDto, now);
                user.LastLogoutAt = now;
            }

            var session = await context.Sessions.FindAsync(sessionDto.SessionId);
            if (session != null)
            {
                session.LogoutAt = now;
            }

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var msg = "ログアウトで例外が発生しました。";
            this._logger.LogError(ex, msg);
        }
    }
}
