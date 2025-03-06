using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// 祝日用のサービスです。
/// </summary>
public interface IHolidayGrpcService : IService<IHolidayGrpcService>
{
    UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync(int year, int month);
}

/// <inheritdoc cref="IHolidayGrpcService"/>
public class HolidayGrpcService : ServiceBase<IHolidayGrpcService>, IHolidayGrpcService
{
    private readonly ILogger _logger;
    private readonly IHolidayService _holidayService;

    public HolidayGrpcService(
        ILogger<HolidayGrpcService> logger,
        IHolidayService holidayService)
    {
        this._logger = logger;
        this._holidayService = holidayService;
    }

    public async UnaryResult<List<HolidayDto>> FetchHolidayDtosAsync(int year, int month)
    {
        return await this._holidayService.GetOrFetchHolidayDtosAsync(year, month);
    }
}
