using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using JetBrains.Annotations;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// 設備予約
/// </summary>
[MessagePackObject]
public class ReservationEquipmentBookingDto
{
    [Key(0)]
    public required int ResvEquipBkgId { get; set; }

    [Key(1)]
    public DateOnly? BkgDate { get; set; }

    [Key(2)]
    public required int EquipId { get; set; }

    [Key(3)]
    public required string Slot { get; set; } = String.Empty;

    [Key(4)]
    public required int BkgUserId { get; set; } = 0;

    [Key(5)]
    public required DateTime BkgAt { get; set; } = DateTime.Now;

    [Key(6)]
    public required string BkgSymbolText { get; set; } = String.Empty;

    [Key(7)]
    public required string BkgRemarkText { get; set; } = String.Empty;

    [Key(8)]
    public required bool IsTentative { get; set; } = false;

    // TODO: JOINしたい
    [Key(9)]
    public required int OrgId { get; set; } = 0;

    [Key(10)]
    public required int PtId { get; set; } = 0;

    [Key(11)]
    public required int RecId { get; set; } = 0;

    [Key(12)]
    public bool IsCancelled { get; set; } = false;

    [Key(13)]
    public bool IsNoShow { get; set; } = false;

    [UsedImplicitly]
    [IgnoreMember]
    public string DisplayText => $"{this.Slot} {this.BkgDate: MM/dd} {this.PtId}";
}

public static class ReservationEquipmentBookingExtensions
{
    public static ReservationEquipmentBookingDto ToReservationEquipmentBookingDto(this ReservationEquipmentBooking booking)
    {
        return new ReservationEquipmentBookingDto
        {
            ResvEquipBkgId = booking.ResvEquipBkgId,
            BkgDate = booking.BkgDate.ToDateOnly(),
            EquipId = booking.EquipId,
            Slot = booking.Slot,
            BkgUserId = booking.BkgUserId,
            BkgAt = booking.BkgAt,
            BkgSymbolText = booking.BkgSymbolText,
            BkgRemarkText = booking.BkgRemarkText,
            IsTentative = booking.IsTentative,
            OrgId = booking.OrgId,
            PtId = booking.PtId,
            RecId = booking.RecId,
            IsCancelled = booking.IsCancelled,
            IsNoShow = booking.IsNoShow,
        };
    }
}
