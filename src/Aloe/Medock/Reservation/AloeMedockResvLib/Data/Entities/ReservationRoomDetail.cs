using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_room_details")]
public class ReservationRoomDetail : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.RoomDetailId;

    [Key]
    [Column("room_detail_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomDetailId { get; set; }

    [Column("room_id")]
    [Required]
    public int RoomId { get; set; } = 0;

    [Column("exam_id")]
    [Required]
    public int ExamId { get; set; } = 0;

    public ReservationRoomDetail() { }

    public ReservationRoomDetail(
        int roomId,
        int examId)
    {
        this.RoomId = roomId;
        this.ExamId = examId;
    }
}
