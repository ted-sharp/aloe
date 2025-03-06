using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;


/// <summary>
/// 設備予約用のサービスです。
/// </summary>
public interface IReservationEquipmentService
{
    Task<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync();

    Task<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId);

    Task<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int? orEquipId);
}

public class ReservationEquipmentService : IReservationEquipmentService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ReservationEquipmentService(
        ILogger<ReservationEquipmentService> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async Task<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync()
    {
        await using var context = await this._factory.CreateDbContextAsync();
        var equips = await context.Equipments
            .AsNoTracking()
            .Where(x => x.IsDeleted == false)
            .OrderBy(x => x.Seq)
            .Select(x => x.ToReservationEquipmentDto())
            .ToListAsync();
        return equips;
    }

    public async Task<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId)
    {
        var firstDate = DateOnlyHelper.GetFirstDate(year, month);
        var endDate = DateOnlyHelper.GetEndDate(firstDate);
        var equipId = orEquipId ?? 0;

        await using var context = await this._factory.CreateDbContextAsync();
        var query = context.EquipmentSlots
            .AsNoTracking()
            .Where(x =>
                x.StartDate <= endDate
                && (x.EndDate == null || firstDate <= x.EndDate)
                && (x.EquipId == 0 || x.EquipId == equipId)
                && x.IsDeleted == false
            )
            .Select(x => new ReservationEquipmentSlotDto
            {
                ResvEquipSlotId = x.ResvEquipSlotId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DowCode = x.DowCode,
                EquipId = x.EquipId,
                Slots = x.SplitSlots(),
            })
            .OrderBy(x => x.StartDate);

        this._logger.LogDebug(query.ToQueryString());

        return await query.ToListAsync();
    }

    public async Task<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int? orEquipId)
    {
        var firstDate = DateOnlyHelper.GetFirstDate(year, month);
        var endDate = DateOnlyHelper.GetEndDate(firstDate);
        var zeroOrEquipId = orEquipId ?? 0;

        await using var context = await this._factory.CreateDbContextAsync();
        var bookings = await context.EquipmentBookings
            .AsNoTracking()
            .Where(x =>
                firstDate <= x.BkgDate
                && x.BkgDate <= endDate
                && x.IsDeleted == false
                && (x.EquipId == 0 || x.EquipId == zeroOrEquipId)
            )
            .Select(x => new ReservationEquipmentBookingDto
            {
                ResvEquipBkgId = x.ResvEquipBkgId,
                EquipId = x.EquipId,
                Slot = x.Slot,
                BkgUserId = x.BkgUserId,
                BkgAt = x.BkgAt,
                BkgDate = x.BkgDate,
                BkgSymbolText = x.BkgSymbolText,
                BkgRemarkText = x.BkgRemarkText,
                IsHeld = x.IsTentative,
                OrgId = x.OrgId,
                PtId = x.PtId,
                OrderId = x.OrderId,
                SubOrderId = x.SubOrderId,
            })
            .ToListAsync();
        return bookings;
    }
}
