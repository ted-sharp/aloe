using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;


[Table("permissions")]
public class Permission : AuditableEntityBase<string>
{
    [NotMapped]
    public override string Id => this.PermCode;

    [Key]
    [Column("perm_code")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PermCode { get; set; } = String.Empty;


    [Column("perm_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PermName { get; set; } = String.Empty;

    [Column("perm_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PermDesc { get; set; } = String.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    public Permission() { }

    public Permission(
        string permCode,
        string permName,
        string permDesc)
    {
        this.PermCode = permCode;
        this.PermName = permName;
        this.PermDesc = permDesc;
    }
}
