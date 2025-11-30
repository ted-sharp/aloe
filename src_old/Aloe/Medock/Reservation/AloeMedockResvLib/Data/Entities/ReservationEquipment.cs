using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_equipments")]
public class ReservationEquipment : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.EquipId;

    [Key]
    [Column("equip_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EquipId { get; set; }

    [Column("equip_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string EquipName { get; set; } = String.Empty;

    [Column("equip_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string EquipDesc { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; } = 0;

    public ReservationEquipment() { }

    public ReservationEquipment(string equipName, string equipDesc, int seq)
    {
        this.EquipName = equipName;
        this.EquipDesc = equipDesc;
        this.Seq = seq;
    }
}
