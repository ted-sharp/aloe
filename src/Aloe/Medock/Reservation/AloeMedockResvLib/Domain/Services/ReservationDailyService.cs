using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Drawing;

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
            .Select(x => new ReservationFloorDto
            {
                FloorId = x.FloorId,
                FloorCode = x.FloorCode,
                FloorName = x.FloorName,
                Seq = x.Seq,
            })
            .ToListAsync();
        return floors;
    }

    public async Task<List<ReservationRoomDto>> FetchRoomDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var rooms = await context.Rooms
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => new ReservationRoomDto
            {
                RoomId = x.RoomId,
                RoomName = x.RoomName,
                Seq = x.Seq,
            })
            .ToListAsync();
        return rooms;
    }

    public async Task<List<ReservationRoomDetailDto>> FetchRoomDetailDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var rooms = await context.RoomDetails
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .Select(x => new ReservationRoomDetailDto
            {
                RoomId = x.RoomId,
                ExamId = x.ExamId,
            })
            .ToListAsync();
        return rooms;
    }

    public async Task<List<ReservationDailySlotDto>> FetchDailySlotDtosAsync(int year, int month)
    {
        var firstDate = DateHelper.GetFirstDateTime(year, month);
        var endDate = DateHelper.GetEndDateTime(firstDate);

        await using var context = await this._factory.CreateDbContextAsync();
        var slots = await context.DailySlots
            .AsNoTracking()
            .Where(x =>
                x.StartDate <= endDate
                && (x.EndDate == null || x.EndDate >= firstDate)
                && x.IsDeleted == false)
            .OrderBy(x => x.StartDate)
            .Select(x => new ReservationDailySlotDto
            {
                ResvDailySlotId = x.ResvDailySlotId,
                StartDate = x.StartDate.ToDateOnly(),
                EndDate = x.EndDate.ToDateOnly(),
                DowCode = x.DowCode,
                FloorId = x.FloorId,
                Slots = x.SplitSlots(),
            })
            .ToListAsync();
        return slots;
    }

    public async Task<List<ReservationDailyNoteDto>> FetchDailyNoteDtosAsync(DateOnly bkgDate, int? orFloorId)
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var floorId = orFloorId ?? 0;
        var date = bkgDate.ToDateTime();

        var notes = await context.DailyNotes
            .AsNoTracking()
            .Where(x =>
                x.BkgDate == date
                && (x.FloorId == 0 || x.FloorId == floorId)
                && x.IsDeleted == false)
            .Select(x => new ReservationDailyNoteDto
            {
                ResvDailyNoteId = x.ResvDailyNoteId,
                BkgDate = x.BkgDate.ToDateOnly(),
                FloorId = x.FloorId,
                NoteText = x.NoteText,
                UpdatedAt = x.UpdatedAt,
                UpdatedUserName = x.UpdatedUserName,
            })
            .ToListAsync();
        return notes;
    }

    public async Task<List<ReservationDailyBookingDto>> FetchDailyBookingDtosAsync(DateOnly bkgDate, int? orFloorId)
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var date = bkgDate.ToDateTime();

        var bookings = await context.DailyBookings
            .AsNoTracking()
            .Where(x =>
                x.BkgDate == date
                && x.IsDeleted == false)
            .Select(x => new ReservationDailyBookingDto
            {
                ResvDailyBkgId = x.ResvDailyBkgId,
                BkgDate = x.BkgDate.ToDateOnly(),
                FloorId = x.FloorId,
                Slot = x.Slot,
                BkgUserId = x.BkgUserId,
                BkgAt = x.BkgAt,
                BkgSymbolText = x.BkgSymbolText,
                BkgRemarkText = x.BkgRemarkText,
                IsTentative = x.IsTentative,
                OrgId = x.OrgId,
                PtId = x.PtId,
                RecId = x.RecId,
                IsCancelled = x.IsCancelled,
                IsNoShow = x.IsNoShow,
            })
            .ToListAsync();
        return bookings;
    }
}
