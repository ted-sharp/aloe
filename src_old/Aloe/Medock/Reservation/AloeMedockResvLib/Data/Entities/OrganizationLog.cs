using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_logs")]
public class OrganizationLog
{
    [Column("org_log_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrgLogId { get; set; }

    [Column("org_id")]
    public required int OrgId { get; set; }

    [Column("action")]
    public required string Action { get; set; }

    [Column("actioned_at")]
    public required DateTime ActionedAt { get; set; } = DateTime.Now;

    [Column("actioned_user_id")]
    [Required]
    public required int ActionedUserId { get; set; } = 0;

    [Column("actioned_user_name")]
    public required string ActionedUserName { get; set; }

    [Column("actioned_session_id")]
    [Required]
    public required Guid ActionedSessionId { get; set; }
}
