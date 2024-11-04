using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;
[Table("reservation_floors")]
public class ReservationFloor
{
    [Key]
    [Column("floor_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FloorId { get; set; }

    [Column("floor_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string FloorName { get; set; } = String.Empty;

    [Column("floor_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string FloorDesc { get; set; } = String.Empty;

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

