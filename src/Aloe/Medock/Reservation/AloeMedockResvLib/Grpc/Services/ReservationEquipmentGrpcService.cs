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
public interface IReservationEquipmentGrpcService : IService<IReservationEquipmentGrpcService>
{
    UnaryResult<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync();

    UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId);

    UnaryResult<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int? orEquipId);
}

public class ReservationEquipmentGrpcService : ServiceBase<IReservationEquipmentGrpcService>, IReservationEquipmentGrpcService
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<AppDbContext> _factory;

    public ReservationEquipmentGrpcService(
        ILogger<ReservationEquipmentGrpcService> logger,
        IDbContextFactory<AppDbContext> factory)
    {
        this._logger = logger;
        this._factory = factory;
    }

    public async UnaryResult<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync()
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

    public async UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId)
    {
        var firstDate = DateOnlyHelper.GetFirstDate(year, month);
        var endDate = DateOnlyHelper.GetEndDate(firstDate);
        var zeroOrEquipId = orEquipId ?? 0;

        await using var context = await this._factory.CreateDbContextAsync();
        var query = context.EquipmentSlots
            .AsNoTracking()
            .Where(x =>
                x.StartDate <= endDate
                && (x.EndDate == null || firstDate <= x.EndDate)
                && x.IsDeleted == false
                && (x.EquipId == 0 || x.EquipId == zeroOrEquipId)
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

    public async UnaryResult<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int? orEquipId)
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
