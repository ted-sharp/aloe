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
/// DBサンプルデータのセットアップ用のサービスです。
/// </summary>
public interface ISeedGrpcService : IService<ISeedGrpcService>
{
    UnaryResult<int> SeedAsync();
}

public class SeedGrpcService : ServiceBase<ISeedGrpcService>, ISeedGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SeedGrpcService(
        ILogger<SeedGrpcService> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async UnaryResult<int> SeedAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        return await context.SeedAsync();
    }
}
