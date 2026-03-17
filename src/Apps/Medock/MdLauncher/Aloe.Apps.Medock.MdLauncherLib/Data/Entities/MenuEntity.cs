// <copyright file="MenuEntity.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aloe.Apps.Medock.MdLauncherLib.Data.Entities;

/// <summary>
/// メニュー項目エンティティ。
/// </summary>
[Table("menus")]
public class MenuEntity
{
    /// <summary>メニューコード (PK)。</summary>
    [Key]
    [Column("menu_code")]
    [MaxLength(10)]
    public string MenuCode { get; set; } = string.Empty;

    /// <summary>メニュー名。</summary>
    [Column("menu_name")]
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    /// <summary>メニュー説明。</summary>
    [Column("menu_desc")]
    [MaxLength(1000)]
    public string MenuDesc { get; set; } = string.Empty;

    /// <summary>表示順。</summary>
    [Column("menu_seq")]
    public int MenuSeq { get; set; }

    /// <summary>アプリコード (FK)。</summary>
    [Column("app_code")]
    [MaxLength(10)]
    public string AppCode { get; set; } = string.Empty;

    /// <summary>有効フラグ。</summary>
    [Column("is_active")]
    public bool IsActive { get; set; }

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
