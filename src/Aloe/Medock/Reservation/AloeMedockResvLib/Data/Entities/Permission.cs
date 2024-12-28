using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;


[Table("permissions")]
public class Permission : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PermId;

    [Key]
    [Column("perm_id")]
    [Required]
    public int PermId { get; set; }

    [Column("perm_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PermName { get; set; } = String.Empty;

    [Column("perm_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PermDesc { get; set; } = String.Empty;

    public Permission() { }

    public Permission(string permName, string permDesc)
    {
        this.PermName = permName;
        this.PermDesc = permDesc;
    }
}
