using AloeReservationGrid.Lib.ReservationLib.Data.EFCore;
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

    UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month);

    UnaryResult<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int equipId);
}

public class ReservationEquipmentGrpcService : ServiceBase<IReservationEquipmentGrpcService>, IReservationEquipmentGrpcService
{
    private readonly ILogger _logger;
    private readonly AppDbContext _context;

    public ReservationEquipmentGrpcService(
        ILogger logger,
        AppDbContext context)
    {
        this._logger = logger;
        this._context = context;
    }

    public async UnaryResult<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync()
    {
        var equips = await this._context.Equipments
            .Where(x => x.IsDeleted == false)
            .Select(x => x.ToReservationEquipmentDto())
            .ToListAsync();
        return equips;
    }

    public async UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month)
    {
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        var slots = await this._context.EquipmentSlots
            .Where(x =>
                x.StartDate <= lastDay
                && x.EndDate >= firstDay
                && x.IsDeleted == false)
            .Select(x => new ReservationEquipmentSlotDto
            {
                ResvEquipSlotId = x.ResvEquipSlotId,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DowCode = x.DowCode,
                EquipId = x.EquipId,
                Slots = x.Slots.Split(',', options),
            })
            .OrderBy(x => x.StartDate)
            .ToListAsync();
        return slots;
    }

    public async UnaryResult<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int equipId)
    {
        var firstDay = new DateTime(year, month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var bookings = await this._context.EquipmentBookings
            .Where(x =>
                firstDay <= x.BkgDate
                && x.BkgDate <= lastDay
                && x.EquipId == equipId
                && x.IsDeleted == false)
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
