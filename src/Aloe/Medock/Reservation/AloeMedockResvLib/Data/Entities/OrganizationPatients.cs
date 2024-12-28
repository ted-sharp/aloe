using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_patients")]
public class OrganizationPatient : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgPtId;

    [Key]
    [Column("org_pt_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrgPtId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("personal_number")]
    [Required]
    public string PersonalNumber { get; set; } = String.Empty;

    [Column("department")]
    [Required]
    public string Department { get; set; } = String.Empty;

    [Column("is_member")]
    [Required]
    public bool IsMember { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime? StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("memo")]
    public string Memo { get; set; } = String.Empty;

    public OrganizationPatient() { }

    public OrganizationPatient(int orgId, int ptId, string personalNumber, string department, bool isMember)
    {
        this.OrgId = orgId;
        this.PtId = ptId;
        this.PersonalNumber = personalNumber;
        this.Department = department;
        this.IsMember = isMember;
    }
}
