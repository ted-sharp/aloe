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
public interface IReservationService : IService<IReservationService>
{
    // それぞれのテーブルのデータの取得
    // 何度もやるので双方向で維持しておきたい
    // reservation_equipments
    // reservation_equipment_slots
    // reservation_equipment_bookings
    // 予約データの更新




    //予約情報の取得
    //public ReservationSummary[] FetchReservationSummaries(DateTime date, int floorId1, int floorId2)


    //UnaryResult<SessionDto> LoginAsync(string user, string pwd);

    //UnaryResult LogoutAsync(SessionDto session);
}


