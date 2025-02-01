using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 検査カテゴリ（exam_categories）
/// </summary>
[Table("exam_categories")]
public class ExamCategory
{
    [Column("exam_cat_id")]
    [Key]
    [Required]
    public int ExamCatId { get; set; }

    [Column("exam_cat_name")]
    [Required]
    public string ExamCatName { get; set; } = String.Empty;

    [Column("exam_cat_short_name")]
    [Required]
    public string ExamCatShortName { get; set; } = String.Empty;

    [Column("exam_cat_desc")]
    [Required]
    public string ExamCatDesc { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; } = 0;

    public ExamCategory() { }

    public ExamCategory(string name, string shortName, string desc)
    {
        this.ExamCatName = name;
        this.ExamCatShortName = shortName;
        this.ExamCatDesc = desc;
    }
}
