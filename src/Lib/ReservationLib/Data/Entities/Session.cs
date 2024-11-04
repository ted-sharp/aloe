using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("sessions")]
public class Session : AuditableEntityBase<Guid>
{
    public override Guid Id => this.SessionId;

    [Key]
    [Column("session_id")]
    [Required]
    public Guid SessionId { get; set; } = Guid.Empty;

    [Column("user_id")]
    [Required]
    public int UserId { get; set; } = 0;

    [Column("user_name")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string UserName { get; set; } = String.Empty;

    [Column("client_app_name")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientAppName { get; set; } = String.Empty;

    [Column("client_endpoint")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientEndpoint { get; set; } = String.Empty;

    [Column("login_at")]
    [Required]
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    [Column("logout_at")]
    public DateTime? LogoutAt { get; set; }
}
