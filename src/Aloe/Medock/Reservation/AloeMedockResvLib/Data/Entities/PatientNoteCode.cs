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

/// <summary>
/// 受診者備考コード
/// </summary>
[Table("patient_note_codes")]
public class PatientNoteCode : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgNoteCode;

    [Column("pt_note_code")]
    [Key]
    [Required]
    public int OrgNoteCode { get; set; }

    [Column("pt_note_name")]
    [Required]
    public string OrgNoteName { get; set; } = String.Empty;

    [Column("pt_note_mark")]
    [Required]
    public string OrgNoteMark { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; }

    public PatientNoteCode() { }

    public PatientNoteCode(int code, string name, string mark, int seq)
    {
        this.OrgNoteCode = code;
        this.OrgNoteName = name;
        this.OrgNoteMark = mark;
        this.Seq = seq;
    }
}
