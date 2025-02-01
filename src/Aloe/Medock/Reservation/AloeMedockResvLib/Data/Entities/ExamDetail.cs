using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 検査詳細（exam_details）
/// </summary>
[Table("exam_details")]
public class ExamDetail
{
    [Column("exam_detail_id")]
    [Key]
    [Required]
    public int ExamDetailId { get; set; }

    [Column("exam_id")]
    [Required]
    public int ExamId { get; set; }

    [Column("obs_id")]
    [Required]
    public int ObsId { get; set; }

    public ExamDetail() { }

    public ExamDetail(int examId, int obsId)
    {
        this.ExamId = examId;
        this.ObsId = obsId;
    }
}
