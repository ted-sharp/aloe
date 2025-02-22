using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("organization_members")]
public class OrganizationMember : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgMbrId;

    [Key]
    [Column("org_mbr_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrgMbrId { get; set; }

    [Column("org_id")]
    [Required]
    public int OrgId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("personal_number")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string PersonalNumber { get; set; } = String.Empty;

    [Column("department")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string Department { get; set; } = String.Empty;

    [Column("is_active")]
    [Required]
    public bool IsActive { get; set; }

    [Column("start_date", TypeName = "Date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date", TypeName = "Date")]
    public DateOnly? EndDate { get; set; }

    [Column("memo")]
    [MaxLength(Int32.MaxValue)]
    public string Memo { get; set; } = String.Empty;

    public OrganizationMember() { }

    public OrganizationMember(int orgId, int ptId, string personalNumber, string department, bool isActive)
    {
        this.OrgId = orgId;
        this.PtId = ptId;
        this.PersonalNumber = personalNumber;
        this.Department = department;
        this.IsActive = isActive;
    }
}
