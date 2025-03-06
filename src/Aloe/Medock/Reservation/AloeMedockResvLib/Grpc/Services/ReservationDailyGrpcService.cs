using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

/// <summary>
/// 日次予約用のサービスです。
/// </summary>
public interface IReservationDailyGrpcService : IService<IReservationDailyGrpcService>
{
    UnaryResult<List<ReservationFloorDto>> FetchFloorDtosAsync();

    UnaryResult<List<ReservationRoomDto>> FetchRoomDtosAsync();

    UnaryResult<List<ReservationRoomDetailDto>> FetchRoomDetailDtosAsync();

    UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(int year, int month);

    UnaryResult<List<ReservationDailyNoteDto>> FetchDailyNoteDtosAsync(DateOnly date, int? orFloorId);

    UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateOnly date, int? orFloorId);
}

public class ReservationDailyGrpcService : ServiceBase<IReservationDailyGrpcService>, IReservationDailyGrpcService
{
    private readonly ILogger _logger;
    private readonly IReservationDailyService _dailyService;

    public ReservationDailyGrpcService(
        ILogger<ReservationDailyGrpcService> logger,
        IReservationDailyService dailyService)
    {
        this._logger = logger;
        this._dailyService = dailyService;
    }

    public async UnaryResult<List<ReservationFloorDto>> FetchFloorDtosAsync()
    {
        return await this._dailyService.FetchFloorDtosAsync();
    }

    public async UnaryResult<List<ReservationRoomDto>> FetchRoomDtosAsync()
    {
        return await this._dailyService.FetchRoomDtosAsync();
    }

    public async UnaryResult<List<ReservationRoomDetailDto>> FetchRoomDetailDtosAsync()
    {
        return await this._dailyService.FetchRoomDetailDtosAsync();
    }

    public async UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(int year, int month)
    {
        return await this._dailyService.FetchDailySlotDtosAsync(year, month);
    }

    public async UnaryResult<List<ReservationDailyNoteDto>> FetchDailyNoteDtosAsync(DateOnly date, int? orFloorId)
    {
        return await this._dailyService.FetchDailyNoteDtosAsync(date, orFloorId);
    }

    public async UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateOnly date, int? orFloorId)
    {
        return await this._dailyService.FetchDailyBookingDtosAsync(date, orFloorId);
    }
}
