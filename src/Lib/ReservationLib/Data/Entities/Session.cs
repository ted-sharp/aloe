using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

/// <summary>
/// 一度のログインでの作業をセッションとして記録します。
/// </summary>
/// <remarks>
/// Session テーブルは共通列を持たないため、<see cref="IAuditableEntity"/> は継承しません。
/// </remarks>
[Table("sessions")]
public class Session
{
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
    public string UserDisplayName { get; set; } = String.Empty;

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
    public DateTime LoginAt { get; set; } = DateTime.Now;

    [Column("logout_at")]
    public DateTime? LogoutAt { get; set; }
}
