using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public required bool IsHeld { get; set; } = false;

    // TODO: JOINしたい
    [Key(9)]
    public required int OrgId { get; set; } = 0;

    [Key(10)]
    public required int PtId { get; set; } = 0;

    [Key(11)]
    public required int OrderId { get; set; } = 0;

    [Key(12)]
    public required int SubOrderId { get; set; } = 0;

    public string GetDisplayText()
    {
        return $"{this.Slot} {this.BkgDate: MM/dd} {this.PtId}";
    }
}
