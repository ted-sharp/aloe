using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Data.Entities;

/// <summary>
/// EFCore で使用するテーブルマッピング用のクラスです。
/// EntityBase は共通列を定義したクラスです。
/// </summary>
public class Sample : AuditableEntityBase<int>
{
    public override int Id => this.SampleId;

    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SampleId { get; set; }

    [Column("name")]
    [Required]
    public required string Name { get; set; }

    [Column("display_name")]
    [Required]
    public required string DisplayName { get; set; }

    [Column("birth_date")]
    [Required]
    public required DateTime? BirthDate { get; set; }
}
