using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("user_roles")]
public class UserRole : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.UserRoleId;

    [Key]
    [Column("user_role_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserRoleId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("role_id")]
    public int RoleId { get; set; } = 0;

    public UserRole() { }

    public UserRole(
        int userId,
        int roleId)
    {
        this.UserId = userId;
        this.RoleId = roleId;
    }
}
