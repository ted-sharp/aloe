using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("cache_updates")]
public class CacheUpdate
{
    [Key]
    [Column("table_name")]
    [Required]
    [StringLength(Int32.MaxValue)]
    public string TableName { get; set; } = String.Empty;

    [Column("latest_updated_at")]
    [Required]
    public DateTime LatestUpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("latest_updated_user_id")]
    [Required]
    public int LatestUpdatedUserId { get; set; } = 0;

    [Column("latest_updated_session_id")]
    [Required]
    public Guid LatestUpdatedSessionId { get; set; }
}
