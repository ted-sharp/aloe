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
/// 団体備考コード
/// </summary>
[Table("organization_note_codes")]
public class OrganizationNoteCode : AuditableEntityBase<int>
{
    [NotMapped]
    public override int Id => this.OrgNoteCode;

    [Column("org_note_code")]
    [Key]
    [Required]
    public int OrgNoteCode { get; set; }

    [Column("org_note_name")]
    [Required]
    public string OrgNoteName { get; set; } = String.Empty;

    [Column("org_note_mark")]
    [Required]
    public string OrgNoteMark { get; set; } = String.Empty;

    [Column("seq")]
    [Required]
    public int Seq { get; set; }

    public OrganizationNoteCode() { }

    public OrganizationNoteCode(int code, string name, string mark, int seq)
    {
        this.OrgNoteCode = code;
        this.OrgNoteName = name;
        this.OrgNoteMark = mark;
        this.Seq = seq;
    }
}
