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

[Table("reservation_daily_slots")]
public class ReservationDailySlot : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailySlotId;

    [Key]
    [Column("resv_daily_slot_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailySlotId { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Column("end_date", TypeName = "Date")]
    public DateTime? EndDate { get; set; }

    [Column("dow_code")]
    [Required]
    public int DowCode { get; set; } = -1;

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("slots")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Slots { get; set; } = String.Empty;

    public ReservationDailySlot() { }

    public ReservationDailySlot(
        DateTime start,
        DowCode dowCode,
        string slots)
    {
        this.StartDate = start;
        this.DowCode = (int)dowCode;
        this.Slots = slots;
    }

    public string[] SplitSlots()
    {
        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        return this.Slots.Split(Delimiter.SlotDelimiter, options);
    }

    public int[] SplitSlotCaps()
    {
        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        return this.Slots.Split(Delimiter.SlotDelimiter, options).Select(x => Int32.Parse(x)).ToArray();
    }
}
