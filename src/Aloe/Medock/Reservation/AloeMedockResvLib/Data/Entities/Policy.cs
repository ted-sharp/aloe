using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

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

    public Policy() { }

    public Policy(string policyCode, string policyName, string dataType, string policyValue, string policyDesc)
    {
        this.PolicyCode = policyCode;
        this.PolicyName = policyName;
        this.DataType = dataType;
        this.PolicyValue = policyValue;
        this.PolicyDesc = policyDesc;
        this.IsActive = true;
    }
}
