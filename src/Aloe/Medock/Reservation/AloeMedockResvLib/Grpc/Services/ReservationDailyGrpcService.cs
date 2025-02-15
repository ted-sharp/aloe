using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
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

    UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(DateTime date);
    UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosBetweenAsync(DateTime startDate, DateTime endDate);

    UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateTime ate);
    UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosBetweenAsync(DateTime startDate, DateTime endDate);
}

public class ReservationDailyGrpcService : ServiceBase<IReservationDailyGrpcService>, IReservationDailyGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ReservationDailyGrpcService(
        ILogger logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async UnaryResult<List<ReservationFloorDto>> FetchFloorDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var floors = await context.Floors
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationFloorDto())
            .ToListAsync();
        return floors;
    }

    public async UnaryResult<List<ReservationRoomDto>> FetchRoomDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var rooms = await context.Rooms
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationRoomDto())
            .ToListAsync();
        return rooms;
    }

    public async UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(DateTime date)
    {
        return await this.FetchDailySlotDtosBetweenAsync(date, date);
    }

    public async UnaryResult<List<ReservationDailySlotDto>> FetchDailySlotDtosBetweenAsync(DateTime startDate, DateTime endDate)
    {
        var firstDay = startDate.Date;
        var lastDay = endDate.Date;

        await using var context = await this._factory.CreateDbContextAsync();
        var slots = await context.DailySlots
            .AsNoTracking()
            .Where(x =>
                x.StartDate <= lastDay
                && x.EndDate >= firstDay
                && x.IsDeleted == false)
            .Select(x => new ReservationDailySlotDto
            {
                ResvDailySlotId = x.ResvDailySlotId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DowCode = x.DowCode,
                FloorId = x.FloorId,
                RoomId = x.RoomId,
                Slots = x.Slots,
            })
            .OrderBy(x => x.StartDate)
            .ToListAsync();
        return slots;
    }

    public async UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateTime date)
    {
        return await this.FetchDailyBookingDtosBetweenAsync(date, date);
    }

    public async UnaryResult<List<ReservationDailyBookingDto>> FetchDailyBookingDtosBetweenAsync(DateTime startDate, DateTime endDate)
    {
        var firstDay = startDate.Date;
        var lastDay = endDate.Date;

        await using var context = await this._factory.CreateDbContextAsync();
        var bookings = await context.DailyBookings
            .AsNoTracking()
            .Where(x =>
                firstDay <= x.BkgDate
                && x.BkgDate <= lastDay
                && x.IsDeleted == false)
            .Select(x => new ReservationDailyBookingDto
            {
                ResvDailyBkgId = x.ResvDailyBkgId,
                FloorId = x.FloorId,
                Slot = x.Slot,
                AmPmCode = x.AmPmCode,
                SexCode = x.SexCode,
                BkgUserId = x.BkgUserId,
                BkgAt = x.BkgAt,
                BkgDate = x.BkgDate,
                BkgSymbolText = x.BkgSymbolText,
                BkgRemarkText = x.BkgRemarkText,
                IsHeld = x.IsTentative,
                OrgId = x.OrgId,
                ResvCount = x.ResvCount,
                PtId = x.PtId,
                OrderId = x.OrderId,
                SubOrderId = x.SubOrderId,
                IsCancelled = x.IsCancelled,
                NoShowCount = x.NoShowCount,
            })
            .ToListAsync();
        return bookings;
    }
}
