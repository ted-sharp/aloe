using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("roles")]
public class Role : AuditableEntityBase<int>
{
    [NotMapped] public override int Id => this.RoleId;

    [Key]
    [Column("role_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoleId { get; set; }

    [Column("role_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string RoleName { get; set; } = String.Empty;

    [Column("role_desc")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string RoleDesc { get; set; } = String.Empty;

    public Role() { }

    public Role(string roleName, string roleDesc)
    {
        this.RoleName = roleName;
        this.RoleDesc = roleDesc;
    }
}
