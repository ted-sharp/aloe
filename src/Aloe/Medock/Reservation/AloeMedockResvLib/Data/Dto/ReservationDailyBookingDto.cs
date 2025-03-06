using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;

/// <summary>
/// 日次予約
/// </summary>
[MessagePackObject]
public class ReservationDailyBookingDto
{
    [Key(0)]
    public required int ResvDailyBkgId { get; set; }

    [Key(1)]
    public DateOnly? BkgDate { get; set; }

    [Key(3)]
    public int FloorId { get; set; } = 0;

    [Key(4)]
    public required string Slot { get; set; } = String.Empty;

    [Key(5)]
    public int AmPmCode { get; set; } = 0;

    [Key(6)]
    public int SexCode { get; set; } = 0;

    [Key(7)]
    public required int BkgUserId { get; set; } = 0;

    [Key(8)]
    public required DateTime BkgAt { get; set; } = DateTime.Now;

    [Key(9)]
    public required string BkgSymbolText { get; set; } = String.Empty;

    [Key(10)]
    public required string BkgRemarkText { get; set; } = String.Empty;

    [Key(11)]
    public required bool IsHeld { get; set; } = false;

    // TODO: JOINしたい
    [Key(12)]
    public required int OrgId { get; set; } = 0;

    [Key(13)]
    public int ResvCount { get; set; } = 0;

    [Key(14)]
    public required int PtId { get; set; } = 0;

    [Key(15)]
    public required int RecId { get; set; } = 0;

    [Key(16)]
    public bool IsCancelled { get; set; } = false;

    [Key(17)]
    public bool IsNoShow { get; set; } = false;
}
