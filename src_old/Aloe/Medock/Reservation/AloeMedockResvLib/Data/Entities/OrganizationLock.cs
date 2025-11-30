using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_locks")]
public class OrganizationLock
{
    [Column("org_lock_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrgLockId { get; set; }

    [Column("org_id")]
    public required int OrgId { get; set; }

    [Column("expiration_at")]
    public DateTime? ExpirationAt { get; set; }

    [Column("locked_screen_code")]
    [Required]
    public required int LockedScreenCode { get; set; } = 0;

    [Column("locked_at")]
    public required DateTime LockedAt { get; set; } = DateTime.Now;

    [Column("locked_user_id")]
    [Required]
    public required int LockedUserId { get; set; } = 0;

    [Column("locked_user_name")]
    public required string LockedUserName { get; set; }

    [Column("locked_session_id")]
    [Required]
    public required Guid LockedSessionId { get; set; }
}
