using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;


public class Role
{
    [Key]
    [Required]
    public int RoleId { get; set; }

    [Required]
    public string RoleName { get; set; } = String.Empty;

    public string RoleDesc { get; set; } = String.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
