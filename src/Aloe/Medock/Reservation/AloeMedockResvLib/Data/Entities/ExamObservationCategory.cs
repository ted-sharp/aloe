using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 検査観察カテゴリ（exam_observation_categories）
/// </summary>
[Table("exam_observation_categories")]
public class ExamObservationCategory
{
    [Column("obs_cat_id")]
    [Key]
    [Required]
    public int ObsCatId { get; set; }

    [Column("obs_cat_name")]
    [Required]
    public string ObsCatName { get; set; } = String.Empty;

    [Column("obs_cat_short_name")]
    [Required]
    public string ObsCatShortName { get; set; } = String.Empty;

    [Column("obs_cat_desc")]
    [Required]
    public string ObsCatDesc { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; } = 0;

    public ExamObservationCategory() { }

    public ExamObservationCategory(string name, string shortName, string desc)
    {
        this.ObsCatName = name;
        this.ObsCatShortName = shortName;
        this.ObsCatDesc = desc;
    }
}
