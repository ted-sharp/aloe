using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("reservation_daily_caps")]
public class ReservationDailyCap : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.ResvDailyCapId;

    [Key]
    [Column("resv_daily_cap_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ResvDailyCapId { get; set; }

    [Column("start_date", TypeName = "Date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Column("end_date", TypeName = "Date")]
    public DateTime? EndDate { get; set; }

    [Column("dow_code")]
    [Required]
    public int DowCode { get; set; } = -1;

    [Column("floor_id")]
    [Required]
    public int FloorId { get; set; } = 0;

    [Column("room_id")]
    [Required]
    public int RoomId { get; set; } = 0;

    [Column("daily_cap")]
    [Required]
    public int DailyCap { get; set; } = 0;

    [Column("am_cap")]
    [Required]
    public int AmCap { get; set; } = 0;

    [Column("pm_cap")]
    [Required]
    public int PmCap { get; set; } = 0;

    public ReservationDailyCap() { }

    public ReservationDailyCap(
        DateTime start,
        DowCode dowCode,
        int ampCap,
        int pmCap)
    {
        this.StartDate = start;
        this.DowCode = (int)dowCode;
        this.DailyCap = ampCap + pmCap;
        this.AmCap = ampCap;
        this.PmCap = pmCap;
    }
}
