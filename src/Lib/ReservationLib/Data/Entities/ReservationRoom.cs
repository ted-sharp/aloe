using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("reservation_rooms")]
public class ReservationRoom : AuditableEntityBase<int>
{
    public override int Id => this.RoomId;

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
}
