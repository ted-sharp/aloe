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
[PrimaryKey(nameof(RolePermission.RoleId), nameof(RolePermission.PermId))]
public class RolePermission : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.RoleId;

    [Column("role_id", Order = 0)]
    public int RoleId { get; set; }

    [Column("perm_id", Order = 1)]
    public int PermId { get; set; } = 0;

    public RolePermission() { }

    public RolePermission(int roleId, int permId)
    {
        this.RoleId = roleId;
        this.PermId = permId;
    }
}
