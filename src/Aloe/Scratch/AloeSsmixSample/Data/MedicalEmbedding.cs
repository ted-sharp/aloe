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
/// ベクトル化医療情報（medical_embeddings）
/// </summary>
[Table("sk.medical_embeddings")]
public class MedicalEmbedding
{
    [Column("embedding_id")]
    [Key]
    [Required]
    public Guid EmbeddingId { get; set; }

    [Column("pt_id")]
    [Required]
    public int PtId { get; set; }

    [Column("source_id")]
    [Required]
    public Guid SourceId { get; set; }

    [Column("source_type")]
    [Required]
    public string SourceType { get; set; } = String.Empty;

    [Column("content")]
    [Required]
    public string Content { get; set; } = String.Empty;

    [Column("embedding")]
    [Required]
    public float[]? Embedding { get; set; } = null;

    [Column("created_at")]
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Today;
}
