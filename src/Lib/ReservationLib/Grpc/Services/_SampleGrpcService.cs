using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Domain.Services;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

public interface ISampleGrpcService : MagicOnion.IService<ISampleGrpcService>
{
    MagicOnion.UnaryResult<SampleDto> FetchSampleAsync();
}

// gRPCで使うサービスとして定義しているのでアプリケーション層となります。
// アプリケーション層ですが、ドメイン層のビジネスロジックを含みます。
// 共通化が必要な場合にのみドメイン層に分離します。
//
// 参照の方向
// Grpc(App) → Data
// Grpc(App) → Domain → Data
// 同プロジェクト内に存在するため、逆転しないよう注意します。
// 必要であればプロジェクトを分割します。
// AloeReservationGrid.Lib.ReservationLib
// AloeReservationGrid.Lib.ReservationLib.Domain
// AloeReservationGrid.Lib.ReservationLib.Grpc
public class SampleGrpcService : MagicOnion.Server.ServiceBase<ISampleGrpcService>, ISampleGrpcService
{
    private readonly ILogger<SampleGrpcService> _logger;
    private readonly AppDbContext _context;
    private readonly ISampleDomainService _domainService;

    public SampleGrpcService(
        ILogger<SampleGrpcService> logger,
        AppDbContext context,
        ISampleDomainService domainService)
    {
        this._logger = logger;
        this._context = context;
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


