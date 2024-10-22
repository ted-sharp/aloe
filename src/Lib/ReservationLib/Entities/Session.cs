using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

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
    public string UserName { get; set; } = String.Empty;

    [Column("client_app_name")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientAppName { get; set; } = String.Empty;

    [Column("client_ip_addr")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientIpAddr { get; set; } = String.Empty;

    [Column("client_mac_addr")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientMacAddr { get; set; } = String.Empty;

    [Column("client_machine_name")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientMachineName { get; set; } = String.Empty;

    [Column("client_machine_guid")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientMachineGuid { get; set; } = String.Empty;

    [Column("client_device_id")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string ClientDeviceId { get; set; } = String.Empty;

    [Column("login_at")]
    [Required]
    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    [Column("logout_at")]
    public DateTime? LogoutAt { get; set; }
}
