using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

[Table("reservation_daily_slots")]
public class ReservationDailySlot
{
    [Key]
    [Column("resv_daily_slot_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailySlotId { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date;

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
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string Slots { get; set; } = String.Empty;

    [Column("slot_caps")]
    [Required]
    [MaxLength(Int32.MaxValue)]  // TEXT型に対応
    public string SlotCaps { get; set; } = String.Empty;

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

