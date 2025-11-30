using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 健診オプション詳細（checkup_option_details）
/// </summary>
[Table("checkup_option_details")]
public class CheckupOptionDetail
{
    [Column("opt_detail_id")]
    [Key]
    [Required]
    public int OptDetailId { get; set; }

    [Column("opt_id")]
    [Required]
    public int OptId { get; set; }

    [Column("exam_id")]
    [Required]
    public int ExamId { get; set; }

    public CheckupOptionDetail() { }

    public CheckupOptionDetail(int optId, int examId)
    {
        this.OptId = optId;
        this.ExamId = examId;
    }
}
