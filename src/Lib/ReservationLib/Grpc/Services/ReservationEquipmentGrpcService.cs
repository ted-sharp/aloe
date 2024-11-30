using System.Diagnostics;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
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
    private readonly AppDbContext _context;

    public ReservationEquipmentGrpcService(
        ILogger<ReservationEquipmentGrpcService> logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    public async UnaryResult<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync()
    {
        var equips = await this._context.Equipments
            .Where(x => x.IsDeleted == false)
            .OrderBy(x => x.Seq)
            .Select(x => x.ToReservationEquipmentDto())
            .ToListAsync();
        return equips;
    }

    public async UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId)
    {
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        var zeroOrEquipId = orEquipId ?? 0;

        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        var query = this._context.EquipmentSlots
            .Where(x =>
                x.StartDate <= lastDay
                && x.EndDate >= firstDay
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
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        var zeroOrEquipId = orEquipId ?? 0;

        var bookings = await this._context.EquipmentBookings
            .Where(x =>
                firstDay <= x.BkgDate
                && x.BkgDate <= lastDay
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
                IsHeld = x.IsHeld,
                OrgId = x.OrgId,
                PtId = x.PtId,
                OrderId = x.OrderId,
                SubOrderId = x.SubOrderId,
            })
            .ToListAsync();
        return bookings;
    }
}
