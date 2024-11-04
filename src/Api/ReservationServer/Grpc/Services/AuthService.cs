using System.Diagnostics;
using MagicOnion;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using System.Numerics;
using System.Text.RegularExpressions;
using AloeReservationGrid.Api.ReservationServer.Data.Repos;
using AloeReservationGrid.Lib.CoreLib.Logging;
using AloeReservationGrid.Lib.CoreLib.Security;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Grpc.Core;
using AloeReservationGrid.Api.ReservationServer.Data.EFCore;
using Microsoft.EntityFrameworkCore;

namespace AloeReservationGrid.Api.ReservationServer.Grpc.Services;

public class AuthService : ServiceBase<IAuthService>, IAuthService
{
    private readonly ILogger _logger;
    private readonly ServerCallContext _rpcContext;
    private readonly AppDbContext _dbContext;

    public AuthService(
        ILogger logger,
        ServerCallContext rpcContext,
        AppDbContext dbContext)
    {
        this._logger = logger;
        this._rpcContext = rpcContext;
        this._dbContext = dbContext;
    }

    public async UnaryResult<LoginResult> LoginAsync(LoginRequest request)
    {
        var now = DateTime.Now;
        var result = new LoginResult
        {
            IsSuccess = false,
            ErrorMessage = null,
            Session = null,
        };

        try
        {
            var user = await this._dbContext.Users
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);
            if (user == null)
            {
                result.ErrorMessage = "ログイン名が正しくありません。";
                return result;
            }

            if (user.LockedUntilAt < now)
            {
                var span = now - user.LockedUntilAt;
                var format = span.ToApproximateJaString();
                result.ErrorMessage = $"ロックされています。(解除まで {format})";
                return result;
            }

            var isVerified = PasswordHasher.Default.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!isVerified)
            {
                result.ErrorMessage = "パスワードが正しくありません。";

                user.FailedCount++;
                // TODO: 基準を超えたらロックして、カウントをリセット
                if (user.FailedCount == 1)
                {
                    // TODO: いつまでロックするのか
                    user.LockedUntilAt = now.AddSeconds(10);
                }
                await this._dbContext.SaveChangesAsync();
                return result;
            }

            if (user.ExpireDate < now.Date)
            {
                result.ErrorMessage = "有効期限を過ぎています。";
                return result;
            }

            var endpoint = this._rpcContext.Peer;
            var session = await this.CreateNewSession(user, request, endpoint);
            if (session == null)
            {
                result.ErrorMessage = "セッションを開始できませんでした。";
                return result;
            }

            var sessionDto = new SessionDto
            {
                SessionId = session.SessionId,
                UserId = user.UserId,
                UserDisplayName = user.DisplayName,
            };

            user.LastLoginAt = now;
            user.SetUpdatedSession(sessionDto, now);
            await this._dbContext.SaveChangesAsync();

            result.IsSuccess = true;
            result.Session = sessionDto;
        }
        catch (Exception ex)
        {
            var msg = "ログインで例外が発生しました。";
            this._logger.Error(ex, msg);

            result.IsSuccess = false;
            result.ErrorMessage = msg;
            result.Session = null;
        }

        return result;
    }

    public async Task<Session> CreateNewSession(User user, LoginRequest request, string endpoint)
    {
        // TODO: セッションを作る
        var newSession = new Session
        {
            SessionId = Guid.NewGuid(),
            UserId = user.UserId,
            UserName = user.DisplayName,
            ClientEndpoint = endpoint,
        };

        var entity = await this._dbContext.Sessions.AddAsync(newSession);
        return entity.Entity;
    }

    public async UnaryResult LogoutAsync(SessionDto sessionDto)
    {
        var now = DateTime.Now;

        try
        {
            var user = await this._dbContext.Users.FindAsync(sessionDto.UserId);
            if (user != null)
            {
                user.SetUpdatedSession(sessionDto, now);
                user.LastLogoutAt = now;
            }

            var session = await this._dbContext.Sessions.FindAsync(sessionDto.SessionId);
            if (session != null)
            {
                session.LogoutAt = now;
            }

            await this._dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var msg = "ログアウトで例外が発生しました。";
            this._logger.Error(ex, msg);
        }
    }
}
