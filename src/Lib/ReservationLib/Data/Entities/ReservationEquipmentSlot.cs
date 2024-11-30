using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("reservation_equipment_slots")]
public class ReservationEquipmentSlot : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvEquipSlotId;

    [Key]
    [Column("resv_equip_slot_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvEquipSlotId { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; } = DateTime.MaxValue.Date;

    [Column("dow_code")]
    [Required]
    public int DowCode { get; set; } = -1;

    [Column("equip_id")]
    [Required]
    public int EquipId { get; set; } = 0;

    [Column("slots")]
    [Required]
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string Slots { get; set; } = String.Empty;

    public ReservationEquipmentSlot() { }

    public ReservationEquipmentSlot(DateTime start, DateTime end, DowCode dowCode, string slots)
    {
        this.StartDate = start.Date;
        this.EndDate = end.Date;
        this.DowCode = (int)dowCode;
        this.Slots = slots;
    }

    public string[] SplitSlots()
    {
        var options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        return this.Slots.Split(Delimiter.SlotDelimiter, options);
    }
}
