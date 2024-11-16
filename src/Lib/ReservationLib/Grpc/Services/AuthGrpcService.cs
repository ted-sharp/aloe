using System.Diagnostics;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using System.Numerics;
using System.Text.RegularExpressions;
using AloeReservationGrid.Lib.CoreLib.Logging;
using AloeReservationGrid.Lib.CoreLib.Security;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Grpc.Core;
using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using AloeReservationGrid.Lib.ReservationLib.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Api.ReservationServer.Grpc.Services;

/// <summary>
/// ログイン, ログアウト用のサービスです。
/// </summary>
public interface IAuthGrpcService : IService<IAuthGrpcService>
{
    UnaryResult TestAsync();

    UnaryResult<LoginResult> LoginAsync(LoginRequest request);

    UnaryResult LogoutAsync(SessionDto session);
}

public class AuthGrpcService : ServiceBase<IAuthGrpcService>, IAuthGrpcService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _context;
    private readonly IPolicyService _policyService;

    public AuthGrpcService(
        ILogger<AuthGrpcService> logger,
        IPolicyService policyService,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
        this._policyService = policyService;
    }

    public async UnaryResult TestAsync()
    {
        //await Task.CompletedTask;
        await this._policyService.LoadPoliciesAsync();
        this._logger.Debug("Test");
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

            var user = await this._context.Users
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);
            if (user == null)
            {
                result.ErrorMessage = "ログイン名が正しくありません。";
                return result;
            }

            if (user.IsLocked(now))
            {
                var span = now - user.LockedUntilAt;
                var format = span.ToApproximateJaString();
                result.ErrorMessage = $"ロックされています。(解除まで {format})";
                return result;
            }

            var isCorrect = user.VerifyPassword(request.Password);
            if (!isCorrect)
            {
                result.ErrorMessage = "パスワードが正しくありません。";

                await this._policyService.LoadPoliciesAsync();
                var lockingFailAttempts = this._policyService.GetValue<int>(PolicyCodes.LoginLockingFailAtempts);
                var lockingSeconds = this._policyService.GetValue<int>(PolicyCodes.LoginLockingSeconds);

                user.FailLogin(
                    lockingFailAttempts,
                    lockingSeconds,
                    DateTime.Now);

                await this._context.SaveChangesAsync();
                return result;
            }

            if (user.IsExpired(now.Date))
            {
                result.ErrorMessage = "有効期限を過ぎています。";
                return result;
            }

            var clientEndpoint = base.Context.CallContext.Peer;
            var session = await this.CreateAndAddNewSessionAsync(
                user, request.ClientAppName, clientEndpoint, now);
            var sessionDto = session.ToSessionDto();

            user.FailedAttemptCount = 0;
            user.LastLoginAt = now;
            user.SetUpdatedSession(sessionDto, now);
            await this._context.SaveChangesAsync();

            result.IsSuccess = true;
            result.SessionDto = sessionDto;
        }
        catch (Exception ex)
        {
            var msg = "ログインで例外が発生しました。";
            this._logger.Error(ex, msg);

            result.IsSuccess = false;
            result.ErrorMessage = msg;
            result.SessionDto = null;
        }

        return result;
    }

    #region Session

    private async Task<Session> CreateAndAddNewSessionAsync(
        User user,
        string clientAppName,
        string clientEndpoint,
        DateTime now)
    {
        // TODO: セッションを作る
        var newSession = new Session
        {
            // TODO: Guid の生成は PostgreSQL側にした方がかぶるリスクがない
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            UserDisplayName = user.DisplayName,
            ClientAppName = clientAppName,
            ClientEndpoint = clientEndpoint,
            LoginAt = now,
            LogoutAt = null,
        };

        var entity = await this._context.Sessions.AddAsync(newSession);
        return entity.Entity;
    }

    #endregion Session

    public async UnaryResult LogoutAsync(SessionDto sessionDto)
    {
        try
        {
            var now = DateTime.Now;

            var user = await this._context.Users.FindAsync(sessionDto.UserId);
            if (user != null)
            {
                user.SetUpdatedSession(sessionDto, now);
                user.LastLogoutAt = now;
            }

            var session = await this._context.Sessions.FindAsync(sessionDto.SessionId);
            if (session != null)
            {
                session.LogoutAt = now;
            }

            await this._context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var msg = "ログアウトで例外が発生しました。";
            this._logger.Error(ex, msg);
        }
    }
}
