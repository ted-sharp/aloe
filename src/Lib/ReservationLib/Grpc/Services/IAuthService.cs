using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MessagePack;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

/// <summary>
/// ログイン, ログアウト用のサービスです。
/// </summary>
public interface IAuthService : IService<IAuthService>
{
    UnaryResult<LoginResult> LoginAsync(LoginRequest request);

    UnaryResult LogoutAsync(SessionDto session);
}
