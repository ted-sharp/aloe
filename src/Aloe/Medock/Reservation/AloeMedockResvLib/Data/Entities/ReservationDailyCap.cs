using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_daily_caps")]
public class ReservationDailyCap : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailyCapId;

    [Key]
    [Column("resv_daily_cap_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailyCapId { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; } = DateTime.Today;

    [Column("dow_code")]
    [Required]
    public int DowCode { get; set; } = 0;

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("room_id")]
    [Required]
    public int RoomId { get; set; } = 0;

    [Column("daily_cap")]
    [Required]
    public int DailyCap { get; set; } = 0;

    [Column("am_cap")]
    [Required]
    public int AmCap { get; set; } = 0;

    [Column("pm_cap")]
    [Required]
    public int PmCap { get; set; } = 0;

    [Column("slots")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Slots { get; set; } = String.Empty;

    [Column("slots_cap")]
    [Required]
    public int SlotsCap { get; set; } = 0;

    public ReservationDailyCap() { }

    public ReservationDailyCap(
        DateTime start,
        DateTime end,
        DowCode dowCode,
        int ampCap,
        int pmCap,
        string slots,
        int slotsCap)
    {
        this.StartDate = start.Date;
        this.EndDate = end.Date;
        this.DowCode = (int)dowCode;
        this.DailyCap = ampCap + pmCap;
        this.AmCap = ampCap;
        this.PmCap = pmCap;
        this.Slots = slots;
        this.SlotsCap = slotsCap;
    }
}
