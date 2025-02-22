using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// 設備予約用のサービスです。
/// </summary>
public interface IHolidayGrpcService : IService<IHolidayGrpcService>
{
    //UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync();

    UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync(int year, int month);
}

public class HolidayGrpcService : ServiceBase<IHolidayGrpcService>, IHolidayGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public HolidayGrpcService(
        ILogger<HolidayGrpcService> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    //public async UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync()
    //{
    //    await using var context = await this._factory.CreateDbContextAsync();
    //    var holidays = await context.Holidays
    //        .AsNoTracking()
    //        .Where(x => x.IsDeleted == false)
    //        .Select(x => x.ToHolidayDto())
    //        .ToListAsync();
    //    return holidays;
    //}

    public async UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync(int year, int month)
    {
        var firstDate = DateOnlyHelper.GetFirstDate(year, month);
        var endDate = DateOnlyHelper.GetEndDate(firstDate);

        await using var context = await this._factory.CreateDbContextAsync();
        var holidays = await context.Holidays
            .AsNoTracking()
            .Where(x =>
                firstDate <= x.HolidayDate
                && x.HolidayDate <= endDate
                && x.IsDeleted == false
            )
            .Select(x => x.ToHolidayDto())
            .ToListAsync();

        return holidays;
    }
}
