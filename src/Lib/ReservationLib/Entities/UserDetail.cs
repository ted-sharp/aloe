using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

public class UserDetail
{
    [Key]
    [Required]
    public int UserId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = String.Empty;

    [Required]
    public string DisplayName { get; set; } = String.Empty;

    [Required]
    public string PasswordHash { get; set; } = String.Empty;

    [Required]
    public JObject UserInfo { get; set; } = new JObject();

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}

