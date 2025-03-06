using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

// サンプルなので使わないフィールドも残します。
// ReSharper disable NotAccessedField.Local

public interface ISampleGrpcService : MagicOnion.IService<ISampleGrpcService>
{
    MagicOnion.UnaryResult<SampleDto> FetchSampleAsync();
}

// gRPCで使うサービスとして定義しているのでExternal Interface/Gateway層となります。
// アプリケーション層の同名のサービスを呼び出す薄いラッパーとして作用します。
//
// 参照の方向
// Grpc(Gateway) → App/Domain
public class SampleGrpcService : MagicOnion.Server.ServiceBase<ISampleGrpcService>, ISampleGrpcService
{
    private readonly ILogger<SampleGrpcService> _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly ISampleDomainService _domainService;

    public SampleGrpcService(
        ILogger<SampleGrpcService> logger,
        IDbContextFactory<AppDbContext> factory,
        ISampleDomainService domainService)
    {
        this._logger = logger;
        this._factory = factory;
        this._domainService = domainService;
    }

    // UnaryResult は Task と同じように await/async できます。
    public async MagicOnion.UnaryResult<SampleDto> FetchSampleAsync()
    {
        await Task.CompletedTask;

        return new SampleDto
        {
            SampleId = 0,
            Name = "",
        };
    }
}



// TODO: どんな処理があればよいか？

// 起動時にマスタデータをある程度キャッシュしておきたい
// キャッシュの仕組みはどうする？
// ログイン時にキャッシュを更新する処理をいれておけば、ログインしなおしでよいかも？

//ログイン、ログアウト
// 頻度が少ない処理は単方向でよさそう
//public Session Login(string user, string pwd, recv)
//public void Logout(Session session, recv)
//マスタデータの更新なども




// それぞれのテーブルのデータの取得
// 何度もやるので双方向で維持しておきたい
// reservation_equipments
// reservation_equipment_slots
// reservation_equipment_bookings
// 予約データの更新




//予約情報の取得
//public ReservationSummary[] FetchReservationSummaries(DateTime date, int floorId1, int floorId2)


