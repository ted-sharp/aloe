using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;

/// <summary>
/// 日次予約用のサービスです。
/// </summary>
public interface IReservationDailyService
{
    Task<List<ReservationFloorDto>> FetchFloorDtosAsync();

    Task<List<ReservationRoomDto>> FetchRoomDtosAsync();

    Task<List<ReservationRoomDetailDto>> FetchRoomDetailDtosAsync();

    Task<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(int year, int month);

    Task<List<ReservationDailyNoteDto>> FetchDailyNoteDtosAsync(DateOnly date, int? orFloorId);

    Task<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateOnly date, int? orFloorId);
}

public class ReservationDailyService : IReservationDailyService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ReservationDailyService(
        ILogger<ReservationDailyService> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async Task<List<ReservationFloorDto>> FetchFloorDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var floors = await context.Floors
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationFloorDto())
            .ToListAsync();
        return floors;
    }

    public async Task<List<ReservationRoomDto>> FetchRoomDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var rooms = await context.Rooms
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationRoomDto())
            .ToListAsync();
        return rooms;
    }

    public async Task<List<ReservationRoomDetailDto>> FetchRoomDetailDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var rooms = await context.RoomDetails
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationRoomDto())
            .ToListAsync();
        return rooms;
    }

    public async Task<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(int year, int month)
    {
        var firstDate = DateOnlyHelper.GetFirstDate(year, month);
        var endDate = DateOnlyHelper.GetEndDate(firstDate);

        await using var context = await this._factory.CreateDbContextAsync();
        var slots = await context.DailySlots
            .AsNoTracking()
            .Where(x =>
                x.StartDate <= endDate
                && (x.EndDate == null || x.EndDate >= firstDate)
                && x.IsDeleted == false)
            .Select(x => new ReservationDailySlotDto
            {
                ResvDailySlotId = x.ResvDailySlotId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DowCode = x.DowCode,
                FloorId = x.FloorId,
                Slots = x.SplitSlots(),
            })
            .OrderBy(x => x.StartDate)
            .ToListAsync();
        return slots;
    }

    public async Task<List<ReservationDailyNoteDto>> FetchDailyNoteDtosAsync(DateOnly date, int? orFloorId)
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var floorId = orFloorId ?? 0;

        var notes = await context.DailyNotes
            .AsNoTracking()
            .Where(x =>
                x.BkgDate == date
                && (x.FloorId == 0 || x.FloorId == floorId)
                && x.IsDeleted == false)
            .Select(x => x.ToReservationDailyNoteDto())
            .ToListAsync();
        return notes;
    }

    public async Task<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateOnly date, int? orFloorId)
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var bookings = await context.DailyBookings
            .AsNoTracking()
            .Where(x =>
                x.BkgDate == date
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
                IsTentative = x.IsTentative,
                OrgId = x.OrgId,
                ResvCount = x.ResvCount,
                PtId = x.PtId,
                RecId = x.RecId,
                IsCancelled = x.IsCancelled,
                IsNoShow = x.IsNoShow,
            })
            .ToListAsync();
        return bookings;
    }
}
