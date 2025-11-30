using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 検査情報（exams）
/// </summary>
[Table("exams")]
public class Exam
{
    [Column("exam_id")]
    [Key]
    [Required]
    public int ExamId { get; set; }

    [Column("exam_cat_id")]
    [Required]
    public int ExamCatId { get; set; }

    [Column("exam_code")]
    [Required]
    public string ExamCode { get; set; } = String.Empty;

    [Column("exam_name")]
    [Required]
    public string ExamName { get; set; } = String.Empty;

    [Column("exam_short_name")]
    [Required]
    public string ExamShortName { get; set; } = String.Empty;

    [Column("exam_abbr_name")]
    [Required]
    public string ExamAbbrName { get; set; } = String.Empty;

    [Column("exam_desc")]
    [Required]
    public string ExamDesc { get; set; } = String.Empty;

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    public Exam() { }

    public Exam(int catId, string code, string name, string shortName, string abbrName, string desc, bool isActive)
    {
        this.ExamCatId = catId;
        this.ExamCode = code;
        this.ExamName = name;
        this.ExamShortName = shortName;
        this.ExamAbbrName = abbrName;
        this.ExamDesc = desc;
        this.IsActive = isActive;
    }
}
