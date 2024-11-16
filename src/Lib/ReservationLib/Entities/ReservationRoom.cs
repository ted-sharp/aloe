using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

[Table("reservation_rooms")]
public class ReservationRoom
{
    [Key]
    [Column("room_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomId { get; set; }

    [Column("room_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string RoomName { get; set; } = String.Empty;

    [Column("room_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string RoomDesc { get; set; } = String.Empty;

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("exam_cat_id")]
    [Required]
    public int ExamCatId { get; set; } = 0;

    [Column("seq")]
    [Required]
    public int Seq { get; set; } = 0;

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
