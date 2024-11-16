using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;


[Table("permissions")]
public class Permission
{
    [Key]
    [Required]
    public int PermissionId { get; set; }

    [Required]
    public string PermissionName { get; set; } = String.Empty;

    public string PermissionDesc { get; set; } = String.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}

