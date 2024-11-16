using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

[Table("patients")]
public class Patient
{
    [Key]
    [Required]
    public int PatientId { get; set; }

    public string KarteNumber { get; set; } = String.Empty;

    public string FullName { get; set; } = String.Empty;

    public string FullNameKatakana { get; set; } = String.Empty;

    public string FullNameKatakanaNormalized { get; set; } = String.Empty;

    public string GivenName { get; set; } = String.Empty;

    public string OldFullName { get; set; } = String.Empty;

    public DateTime BirthDate { get; set; } = DateTime.UtcNow;

    public int SexCode { get; set; } = 0;

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int UpdatedUserId { get; set; } = 0;

    public Guid UpdatedSessionId { get; set; } = Guid.Empty;
}
