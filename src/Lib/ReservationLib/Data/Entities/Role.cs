using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("roles")]

public class Role
{
    [Key]
    [Required]
    public int RoleId { get; set; }

    [Required]
    public string RoleName { get; set; } = String.Empty;

    public string RoleDesc { get; set; } = String.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
