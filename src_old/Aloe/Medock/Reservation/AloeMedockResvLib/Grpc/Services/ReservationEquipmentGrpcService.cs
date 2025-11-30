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
    private readonly IReservationEquipmentService _equipmentService;

    public ReservationEquipmentGrpcService(
        ILogger<ReservationEquipmentGrpcService> logger,
        IReservationEquipmentService equipmentService)
    {
        this._logger = logger;
        this._equipmentService = equipmentService;
    }

    public async UnaryResult<List<ReservationEquipmentDto>> FetchEquipmentDtosAsync()
    {
        return await this._equipmentService.FetchEquipmentDtosAsync();
    }

    public async UnaryResult<List<ReservationEquipmentSlotDto>> FetchEquipmentSlotDtosAsync(int year, int month, int? orEquipId)
    {
        return await this._equipmentService.FetchEquipmentSlotDtosAsync(year, month, orEquipId);
    }

    public async UnaryResult<List<ReservationEquipmentBookingDto>> FetchEquipmentBookingDtosAsync(int year, int month, int? orEquipId)
    {
        return await this._equipmentService.FetchEquipmentBookingDtosAsync(year, month, orEquipId);
    }
}
