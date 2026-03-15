// <copyright file="Patient.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdPatientLib.Data.Entities;

/// <summary>
/// 患者エンティティ。
/// </summary>
[Table("patients")]
public class Patient
{
    /// <summary>患者 ID (PK)。</summary>
    [Key]
    [Column("pt_id")]
    public Guid PtId { get; set; }

    /// <summary>正規患者 ID（名寄せ検索用）。</summary>
    [Column("canonical_pt_id")]
    public Guid CanonicalPtId { get; set; }

    /// <summary>施設 ID (FK)。</summary>
    [Column("facility_id")]
    public Guid FacilityId { get; set; }

    /// <summary>主団体 ID。</summary>
    [Column("primary_org_id")]
    public Guid PrimaryOrgId { get; set; }

    /// <summary>患者コード。</summary>
    [Column("pt_code")]
    [MaxLength(100)]
    public string PtCode { get; set; } = string.Empty;

    /// <summary>カルテコード。</summary>
    [Column("karte_code")]
    [MaxLength(100)]
    public string? KarteCode { get; set; }

    /// <summary>患者名。</summary>
    [Column("pt_name")]
    [MaxLength(100)]
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名（互換）。</summary>
    [Column("pt_name_compat")]
    [MaxLength(100)]
    public string PtNameCompat { get; set; } = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [Column("pt_name_katakana")]
    [MaxLength(100)]
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>患者名カタカナ（互換）。</summary>
    [Column("pt_name_katakana_compat")]
    [MaxLength(100)]
    public string PtNameKatakanaCompat { get; set; } = string.Empty;

    /// <summary>旧姓名（名寄せなどで使用）。</summary>
    [Column("pt_maiden_name")]
    [MaxLength(100)]
    public string PtMaidenName { get; set; } = string.Empty;

    /// <summary>別名・芸名（印刷用）。</summary>
    [Column("pt_alias_name")]
    [MaxLength(100)]
    public string PtAliasName { get; set; } = string.Empty;

    /// <summary>生年月日。</summary>
    [Column("birth_date")]
    public DateOnly BirthDate { get; set; }

    /// <summary>性別コード (0: None, 1: Man, 2: Woman, 9: Unknown)。</summary>
    [Column("sex_code")]
    public int SexCode { get; set; }

    /// <summary>メモ。</summary>
    [Column("pt_memo")]
    [MaxLength(1000)]
    public string PtMemo { get; set; } = string.Empty;

    /// <summary>削除フラグ。</summary>
    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    /// <summary>作成日時。</summary>
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>更新日時。</summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
