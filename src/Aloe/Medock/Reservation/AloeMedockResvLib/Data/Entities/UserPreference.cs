using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("user_preferences")]
[PrimaryKey(nameof(UserPreference.UserId), nameof(UserPreference.PrefCode))]
public class UserPreference : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.UserId;

    [Column("user_id", Order = 0)]
    public int UserId { get; set; }

    [Column("pref_code", Order = 1)]
    public string PrefCode { get; set; } = String.Empty;

    [Column("pref_value")]
    public string PrefValue { get; set; } = String.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    public UserPreference() { }

    public UserPreference(int userId, string prefCode, string prefValue, bool isActive)
    {
        this.UserId = userId;
        this.PrefCode = prefCode;
        this.PrefValue = prefValue;
        this.IsActive = isActive;
    }
}
