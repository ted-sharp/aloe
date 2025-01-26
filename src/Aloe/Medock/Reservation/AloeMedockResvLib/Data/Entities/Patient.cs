using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

[Table("patients")]
public class Patient : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.PtId;

    [Key]
    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("karte_number")]
    [Required]
    public string KarteNumber { get; set; } = String.Empty;

    [Column("pt_full_name")]
    [Required]
    public string FullName { get; set; } = String.Empty;

    [Column("pt_full_name_katakana")]
    [Required]
    public string FullNameKatakana { get; set; } = String.Empty;

    [Column("pt_full_name_katakana_normalized")]
    [Required]
    public string FullNameKatakanaNormalized { get; set; } = String.Empty;

    [Column("pt_given_name")]
    [Required]
    public string GivenName { get; set; } = String.Empty;

    [Column("pt_former_full_name")]
    [Required]
    public string FormerFullName { get; set; } = String.Empty;

    [Column("birth_date")]
    [Required]
    public DateTime BirthDate { get; set; } = DateTime.MinValue.Date;

    [Column("sex_code")]
    [Required]
    public int SexCode { get; set; } = 0;

    [Column("memo")]
    [Required]
    public string Memo { get; set; } = String.Empty;

    public Patient() { }

    public Patient(string karteNumber, string fullName, string katakana, DateTime birthDate, int sexCode)
    {
        this.KarteNumber = karteNumber;
        this.FullName = fullName;
        this.FullNameKatakana = katakana;
        this.GivenName = fullName; // TODO: 名前部分を切り取って入れる
        this.BirthDate = birthDate;
        this.SexCode = sexCode;
    }
}
