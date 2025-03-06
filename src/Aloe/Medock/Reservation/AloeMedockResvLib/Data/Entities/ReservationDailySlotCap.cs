using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_daily_slot_caps")]
public class ReservationDailySlotCap : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailyCapId;

    [Key]
    [Column("resv_daily_slot_cap_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailyCapId { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateOnly StartDate { get; set; } = DateOnlyHelper.GetToday();

    [Column("end_date", TypeName = "Date")]
    public DateOnly? EndDate { get; set; }

    [Column("dow_code")]
    [Required]
    public int DowCode { get; set; } = -1;

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("room_id")]
    [Required]
    public int RoomId { get; set; } = 0;

    [Column("slots")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Slots { get; set; } = String.Empty;

    [Column("slots_cap")]
    [Required]
    public int SlotsCap { get; set; } = 0;

    public ReservationDailySlotCap() { }

    public ReservationDailySlotCap(
        DateOnly start,
        DowCode dowCode,
        string slots,
        int slotsCap)
    {
        this.StartDate = start;
        this.DowCode = (int)dowCode;
        this.Slots = slots;
        this.SlotsCap = slotsCap;
    }
}
