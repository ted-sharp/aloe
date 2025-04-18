using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Util;

namespace AloeSsmixSample.Data;

/// <summary>
/// SSMIXソース（ssmix_sources）
/// </summary>
[Table("sk.ssmix_sources")]
public class SsmixSource
{
    [Column("source_id")]
    [Key]
    [Required]
    public Guid SourceId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("source_file")]
    [Required]
    public string SourceFile { get; set; } = String.Empty;

    [Column("section_type")]
    [Required]
    public string SectionType { get; set; } = String.Empty;

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Today;
}
