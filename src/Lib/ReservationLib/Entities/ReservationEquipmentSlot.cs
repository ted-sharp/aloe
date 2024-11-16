using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

[Table("reservation_equipment_slots")]
public class ReservationEquipmentSlot
{
    [Key]
    [Column("resv_equip_slot_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvEquipSlotId { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date;

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

    [Column("is_deleted")]
    [Required]
    public bool IsDeleted { get; set; } = false;

    [Column("updated_at")]
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_user_id")]
    [Required]
    public int UpdatedUserId { get; set; } = 0;

    [Column("updated_session_id")]
    [Required]
    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
