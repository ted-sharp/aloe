using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 検査観察（exam_observations）
/// </summary>
[Table("exam_observations")]
public class ExamObservation
{
    [Column("obs_id")]
    [Key]
    [Required]
    public int ObsId { get; set; }

    [Column("obs_cat_id")]
    [Required]
    public int ObsCatId { get; set; }

    [Column("obs_code")]
    [Required]
    public string ObsCode { get; set; } = String.Empty;

    [Column("obs_name")]
    [Required]
    public string ObsName { get; set; } = String.Empty;

    [Column("obs_short_name")]
    [Required]
    public string ObsShortName { get; set; } = String.Empty;

    [Column("obs_abbr_name")]
    [Required]
    public string ObsAbbrName { get; set; } = String.Empty;

    [Column("obs_desc")]
    [Required]
    public string ObsDesc { get; set; } = String.Empty;

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    [Required]
    public DateTime EndDate { get; set; }

    public ExamObservation() { }

    public ExamObservation(
        int obsCatId,
        string obsCode,
        string obsName,
        string obsShortName,
        string obsAbbrName,
        string obsDesc,
        bool isActive,
        DateTime startDate,
        DateTime endDate)
    {
        this.ObsCatId = obsCatId;
        this.ObsCode = obsCode;
        this.ObsName = obsName;
        this.ObsShortName = obsShortName;
        this.ObsAbbrName = obsAbbrName;
        this.ObsDesc = obsDesc;
        this.IsActive = isActive;
        this.StartDate = startDate;
        this.EndDate = endDate;
    }
}
