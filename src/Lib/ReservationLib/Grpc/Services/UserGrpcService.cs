using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.CoreLib.Security;
using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

/// <summary>
/// ユーザー用のサービスです。
/// </summary>
public interface IUserGrpcService : IService<IUserGrpcService>
{
    UnaryResult<UserRegisterResult> RegisterUserAsync(UserRegisterRequest request);
}

public class UserGrpcService : ServiceBase<IUserGrpcService>, IUserGrpcService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _context;

    public UserGrpcService(
        ILogger<UserGrpcService> logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    public async UnaryResult<UserRegisterResult> RegisterUserAsync(UserRegisterRequest request)
    {
        var result = new UserRegisterResult
        {
            IsSuccess = false,
            ErrorMessage = null,
            UserDto = null,
        };

        try
        {
            var now = DateTime.Now;

            var session = await this._context.Sessions
                .FirstOrDefaultAsync(x => x.SessionId == request.SessionId);
            if (session == null)
            {
                result.ErrorMessage = "セッションがありません。";
                return result;
            }

            if (session.LogoutAt != null)
            {
                result.ErrorMessage = "セッションが無効です。";
                return result;
            }

            var user = await this._context.Users
                .FirstOrDefaultAsync(x => x.LoginName == request.LoginName);
            if (user != null)
            {
                result.ErrorMessage = "すでにユーザーが存在しています。";
                result.UserDto = user.ToUserDto();
                return result;
            }

            var newUser = await this.CreateAndAddNewUserAsync(request, session, now);
            result.UserDto = newUser.ToUserDto();
            await this._context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            var msg = "ユーザー登録で例外が発生しました。";
            this._logger.LogError(ex, msg);

            result.IsSuccess = false;
            result.ErrorMessage = msg;
            result.UserDto = null;
        }

        return result;
    }

    #region Session

    private async Task<User> CreateAndAddNewUserAsync(
        UserRegisterRequest request,
        Session session,
        DateTime now)
    {
        var (hash, salt) = PasswordHasher.Default.HashPassword(request.Password);

        var newUser = new User
        {
            LoginName = request.LoginName,
            Email = request.Email,
            PasswordHash = hash,
            PasswordSalt = salt,
            DisplayName = request.DisplayName,

            // TODO: ポリシーから設定する
            ExpireDate = DateTime.Today.AddYears(1),
            FailedAttemptCount = 0,
            LockedUntilAt = now,
            LastLoginAt = now,
            LastLogoutAt = now,
        };

        var entity = await this._context.Users.AddAsync(newUser);
        return entity.Entity;
    }

    #endregion Session

}
