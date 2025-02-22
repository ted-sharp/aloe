using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("holidays")]
public class Holiday : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.HolidayId;

    [Key]
    [Column("holiday_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HolidayId { get; set; }

    [Column("holiday_date", TypeName = "Date")]
    [Required]
    public DateOnly HolidayDate { get; set; } = DateOnly.MinValue;

    [Column("holiday_name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public string HolidayName { get; set; } = String.Empty;

    public Holiday() { }

    public Holiday(DateOnly date, string name)
    {
        this.HolidayDate = date;
        this.HolidayName = name;
    }
}
