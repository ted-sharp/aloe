using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("role_permissions")]
public class RolePermission : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.RolePermId;

    [Key]
    [Column("role_perm_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RolePermId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("perm_code")]
    [MaxLength(Int32.MaxValue)]
    public string PermCode { get; set; } = String.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    public RolePermission() { }

    public RolePermission(
        int roleId,
        string permCode,
        bool isActivate)
    {
        this.RoleId = roleId;
        this.PermCode = permCode;
        this.IsActive = isActivate;
    }
}
