using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Entities;

public class Organization
{
    [Key]
    [Required]
    public int OrganizationId { get; set; }

    public string InsuranceTypeCode { get; set; } = String.Empty;

    public int InsuranceProviderId { get; set; } = 0;

    public int ParentOrganizationId { get; set; } = 0;

    public string OrganizationName { get; set; } = String.Empty;

    public string OrganizationNameKatakana { get; set; } = String.Empty;

    public string OrganizationNameKatakanaNormalized { get; set; } = String.Empty;

    public string OrganizationNameDisplay { get; set; } = String.Empty;

    public string OrganizationNamePrint { get; set; } = String.Empty;

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
