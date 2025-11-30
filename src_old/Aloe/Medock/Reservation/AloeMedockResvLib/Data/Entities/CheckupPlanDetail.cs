using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// 健診プラン詳細（checkup_plan_details）
/// </summary>
[Table("checkup_plan_details")]
public class CheckupPlanDetail
{
    [Column("plan_detail_id")]
    [Key]
    [Required]
    public int PlanDetailId { get; set; }

    [Column("plan_id")]
    [Required]
    public int PlanId { get; set; }

    [Column("exam_id")]
    [Required]
    public int ExamId { get; set; }

    public CheckupPlanDetail() { }

    public CheckupPlanDetail(int planId, int examId)
    {
        this.PlanId = planId;
        this.ExamId = examId;
    }
}
