using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("users")]
public class User : AuditableEntityBase<int>
{
    public override int Id => this.UserId;

    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Required]
    public string DisplayName { get; set; } = String.Empty;

    [Column("login_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string LoginName { get; set; } = String.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = String.Empty;

    [Column("password_hash")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordHash { get; set; } = String.Empty;

    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PasswordSalt { get; set; } = String.Empty;

    public DateTime ExpireDate { get; set; } = DateTime.UtcNow;

    public int FailedCount { get; set; } = 0;

    public DateTime LockedUntilAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public DateTime LastLogoutAt { get; set; } = DateTime.UtcNow;

    [Required]
    public JObject UserInfo { get; set; } = new JObject();
}





