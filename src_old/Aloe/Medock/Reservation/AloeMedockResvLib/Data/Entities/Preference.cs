using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("preferences")]
public class Preference : AuditableEntityBase<string>
{
    [NotMapped]
    public override string Id => this.PrefCode;

    [Key]
    [Required]
    [Column("pref_code")]
    public string PrefCode { get; set; } = String.Empty;

    [Column("pref_name")]
    public string PrefName { get; set; } = String.Empty;

    [Column("pref_desc")]
    public string PrefDesc { get; set; } = String.Empty;

    [Column("data_type")]
    public string DataType { get; set; } = String.Empty;

    [Column("pref_value")]
    public string PrefValue { get; set; } = String.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = false;

    public Preference() { }

    //public Preference(string prefCode, string prefName, string prefDesc, string dataType, string prefValue)
    //{
    //    this.PrefCode = prefCode;
    //    this.PrefName = prefName;
    //    this.PrefDesc = prefDesc;
    //    this.DataType = dataType;
    //    this.PrefValue = prefValue;
    //    this.IsActive = true;
    //}
}
