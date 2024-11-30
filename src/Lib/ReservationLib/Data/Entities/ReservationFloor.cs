using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("reservation_floors")]
public class ReservationFloor : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.FloorId;

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

    public ReservationFloor() { }

    public ReservationFloor(string name, string desc, int seq)
    {
        this.FloorName = name;
        this.FloorDesc = desc;
        this.Seq = seq;
    }
}
