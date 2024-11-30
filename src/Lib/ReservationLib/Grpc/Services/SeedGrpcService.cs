using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Data.Entities;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using AloeReservationGrid.Lib.ReservationLib.Domain.Services;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;

/// <summary>
/// DBサンプルデータのセットアップ用のサービスです。
/// </summary>
public interface ISeedGrpcService : IService<ISeedGrpcService>
{
    UnaryResult<int> SeedAsync();
}

public class SeedGrpcService : ServiceBase<ISeedGrpcService>, ISeedGrpcService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _context;

    public SeedGrpcService(
        ILogger<SeedGrpcService> logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    public async UnaryResult<int> SeedAsync()
    {
        return await this._context.SeedAsync();
    }
}
