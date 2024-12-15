using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// ログイン, ログアウト用のサービスです。
/// </summary>
public interface IAuthGrpcService : IService<IAuthGrpcService>
{
    UnaryResult LoadPoliciesAsync();

    UnaryResult<LoginResult> LoginAsync(LoginRequest request);

    UnaryResult LogoutAsync(SessionDto session);
}

public class AuthGrpcService : ServiceBase<IAuthGrpcService>, IAuthGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IPolicyService _policyService;

    public AuthGrpcService(
        ILogger<AuthGrpcService> logger,
        IDbContextFactory<AppDbContext> factory,
        IPolicyService policyService)
    {
        this._logger = logger;
        this._factory = factory;
        this._policyService = policyService;
    }

    public async UnaryResult LoadPoliciesAsync()
    {
        await this._policyService.LoadPoliciesAsync();
    }

    public async UnaryResult<LoginResult> LoginAsync(LoginRequest request)
    {
        var result = new LoginResult
        {
            IsSuccess = false,
            ErrorMessage = null,
            SessionDto = null,
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

                await this._policyService.LoadPoliciesAsync();
                var lockingFailAttempts = this._policyService.GetValue<int>(PolicyCode.LoginLockingFailAtempts);
                var lockingSeconds = this._policyService.GetValue<int>(PolicyCode.LoginLockingSeconds);

                user.FailLogin(
                    lockingFailAttempts,
                    lockingSeconds,
                    DateTime.Now);

                await context.SaveChangesAsync();
                return result;
            }

            if (user.IsExpired(now.Date))
            {
                result.ErrorMessage = "有効期限を過ぎています。";
                return result;
            }

            var clientEndpoint = base.Context?.CallContext.Peer ?? "";
            var session = await this.CreateAndAddNewSessionAsync(
                context, user, request.ClientAppName, clientEndpoint, now);
            var sessionDto = session.ToSessionDto();

            user.FailedAttemptCount = 0;
            user.LastLoginAt = now;
            user.SetUpdatedSession(sessionDto, now);
            await context.SaveChangesAsync();

            result.IsSuccess = true;
            result.SessionDto = sessionDto;
        }
        catch (Exception ex)
        {
            var msg = "ログインで例外が発生しました。";
            this._logger.LogError(ex, msg);

            result.IsSuccess = false;
            result.ErrorMessage = msg;
            result.SessionDto = null;
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

    public async UnaryResult LogoutAsync(SessionDto sessionDto)
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
