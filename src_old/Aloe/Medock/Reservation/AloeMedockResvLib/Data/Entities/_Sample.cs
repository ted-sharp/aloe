using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;

/// <summary>
/// EFCore で使用するテーブルマッピング用のクラスです。
/// <see cref="AuditableEntityBase{TKey}"/> は共通列を定義したクラスです。
/// </summary>
[Table("samples")]
public class Sample : AuditableEntityBase<int>
{
    public override int Id => this.SampleId;

    [Column("id")]
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SampleId { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(Int32.MaxValue)]
    public required string Name { get; set; }
}
