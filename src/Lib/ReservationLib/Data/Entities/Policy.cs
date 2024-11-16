using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("policies")]
public class Policy : AuditableEntityBase<string>
{
    [NotMapped]
    public override string Id => this.PolicyCode;

    [Key]
    [Required]
    [Column("policy_code")]
    public string PolicyCode { get; set; } = String.Empty;

    [Column("policy_name")]
    public string PolicyName { get; set; } = String.Empty;

    [Column("data_type")]
    public string DataType { get; set; } = String.Empty;

    [Column("policy_value")]
    public string PolicyValue { get; set; } = String.Empty;

    [Column("policy_desc")]
    public string PolicyDesc { get; set; } = String.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;
}
