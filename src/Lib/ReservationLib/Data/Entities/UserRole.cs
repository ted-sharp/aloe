using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;


[Table("user_roles")]
public class UserRole
{
    [Key]
    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoleId { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
